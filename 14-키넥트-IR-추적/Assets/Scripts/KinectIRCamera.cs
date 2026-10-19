using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class KinectIRSticker : MonoBehaviour
{
    [DllImport("KinectIRPluginFinal")] static extern bool InitKinectIR();
    [DllImport("KinectIRPluginFinal")] static extern bool GetIRFrame(IntPtr buffer);
    [DllImport("KinectIRPluginFinal")] static extern bool InitKinectDepth();
    [DllImport("KinectIRPluginFinal")] static extern bool GetDepthFrame(IntPtr buffer);
    [DllImport("KinectIRPluginFinal")] static extern void CloseKinect();

    [Header("UI Components")]
    public RawImage rawImage;
    public Text posText;

    [Header("Detection Parameters")]
    public int minDistanceMM = 200;
    public int maxDistanceMM = 2500;

    [Header("Brightness Detection")]
    [Range(100, 255)]
    public byte brightHi = 200;  // 각도 틀어진 밝은 반사용
    [Range(0, 100)]
    public byte darkLo = 30;     // 정면 어두운 반사용
    [Range(80, 180)]
    public byte midRange = 120;  // 중간 밝기 범위

    [Header("Neighbor Analysis")]
    [Range(1, 5)]
    public int neighRadius = 3;  // 더 넓은 범위로 검사
    [Range(3, 15)]
    public int neighMin = 4;     // 최소 요구값 낮춤
    [Range(15, 50)]
    public int neighMax = 35;    // 최대값 증가

    [Header("Visual")]
    [Range(3, 10)]
    public int crossLen = 7;

    [Header("Stabilization")]
    [Range(0.1f, 1.0f)]
    public float smoothingFactor = 0.3f;  // 좌표 평활화
    [Range(3, 20)]
    public int historySize = 10;          // 이력 크기
    public float maxJumpDistance = 50f;   // 최대 점프 거리

    // 추가된 검출 파라미터
    [Header("Advanced Detection")]
    [Range(100, 255)]
    public byte midBright = 130;          // 중간 밝기 임계값
    [Range(0.5f, 3.0f)]
    public float contrastThreshold = 1.5f; // 대비 임계값
    [Range(5, 50)]
    public int minBlobSize = 8;           // 최소 블롭 크기
    [Range(10, 100)]
    public int intensityJumpThreshold = 40; // 밝기 점프 임계값 (각도 변화 감지)

    Texture2D tex;
    ushort[] ir, depth;
    Color32[] buf;
    const int W = 512, H = 424;

    // 좌표 안정화를 위한 변수들
    private Queue<Vector2> positionHistory = new Queue<Vector2>();
    public Vector2 smoothedPosition = Vector2.zero;
    public Vector2 lastValidPosition = Vector2.zero;
    public bool hasValidPosition = false;
    private int framesSinceLastDetection = 0;

    void Start()
    {
        if (!InitKinectIR() || !InitKinectDepth())
        {
            Debug.LogError("Kinect 초기화 실패");
            enabled = false;
            return;
        }

        ir = new ushort[W * H];
        depth = new ushort[W * H];
        buf = new Color32[W * H];

        tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        if (rawImage != null) rawImage.texture = tex;
        if (posText != null) posText.text = "N/A";
    }

    void Update()
    {
        var h1 = GCHandle.Alloc(ir, GCHandleType.Pinned);
        var h2 = GCHandle.Alloc(depth, GCHandleType.Pinned);

        bool ok1 = GetIRFrame(h1.AddrOfPinnedObject());
        bool ok2 = GetDepthFrame(h2.AddrOfPinnedObject());
        if (!ok1 || !ok2)
        {
            h1.Free();
            h2.Free();
            return;
        }

        // 다중 검출 방식 적용
        Vector2? detectedPos = DetectReflectiveSticker();

        if (detectedPos.HasValue)
        {
            Vector2 newPos = detectedPos.Value;

            // 좌표 검증 및 안정화
            if (IsValidPosition(newPos))
            {
                smoothedPosition = ApplySmoothing(newPos);
                AddToHistory(smoothedPosition);
                lastValidPosition = smoothedPosition;
                hasValidPosition = true;
                framesSinceLastDetection = 0;

                DrawCross(buf, W, H, (int)smoothedPosition.x, (int)smoothedPosition.y, crossLen);

                if (posText != null)
                {
                    posText.text = string.Format("({0:F1}, {1:F1}) px", smoothedPosition.x, smoothedPosition.y);
                }
            }
        }
        else
        {
            framesSinceLastDetection++;

            // 짧은 시간 동안 마지막 위치 유지
            if (framesSinceLastDetection < 10 && hasValidPosition)
            {
                DrawCross(buf, W, H, (int)lastValidPosition.x, (int)lastValidPosition.y, crossLen);
                if (posText != null)
                {
                    posText.text = string.Format("({0:F1}, {1:F1}) px [hold]", lastValidPosition.x, lastValidPosition.y);
                }
            }
            else
            {
                if (posText != null) posText.text = "N/A";
                hasValidPosition = false;
            }
        }

        tex.SetPixels32(buf);
        tex.Apply(false);

        h1.Free();
        h2.Free();
    }

    Vector2? DetectReflectiveSticker()
    {
        List<Vector2> candidates = new List<Vector2>();

        // 1. 정면 어두운 반사 검출 (기존 방식)
        Vector2? darkReflection = DetectDarkReflection();
        if (darkReflection.HasValue) candidates.Add(darkReflection.Value);

        // 2. 각도 틀어진 밝은 반사 검출 (새로운 방식)
        Vector2? brightReflection = DetectAngledBrightReflection();
        if (brightReflection.HasValue) candidates.Add(brightReflection.Value);

        // 3. 밝기 점프 기반 검출 (반사 특성 활용)
        Vector2? jumpDetection = DetectByIntensityJump();
        if (jumpDetection.HasValue) candidates.Add(jumpDetection.Value);

        // 4. 적응형 임계값 검출
        Vector2? adaptiveDetection = DetectByAdaptiveThreshold();
        if (adaptiveDetection.HasValue) candidates.Add(adaptiveDetection.Value);

        // 후보들 중 최적 선택
        return SelectBestCandidate(candidates);
    }

    // 정면 어두운 반사 검출 (기존 방식 개선)
    Vector2? DetectDarkReflection()
    {
        long sumX = 0, sumY = 0, sumW = 0;

        for (int y = 0; y < H; y++)
        {
            int row = y * W;
            int inv = (H - 1 - y) * W;

            for (int x = 0; x < W; x++)
            {
                int idx = row + x;
                int idxInv = inv + x;

                byte g = (byte)(ir[idx] >> 8);
                buf[idxInv] = new Color32(g, g, g, 255);

                ushort d = depth[idx];
                if (d == 0 || d < minDistanceMM || d > maxDistanceMM) continue;

                // 정면 반사: 어두운 중심부 + 밝은 주변부
                bool isDarkCenter = g <= darkLo && HasBrightNeighbors(x, y, neighRadius, 6);
                if (!isDarkCenter) continue;

                int brightNeighborCount = CountBrightNeighbors(x, y, neighRadius, brightHi);
                if (brightNeighborCount < neighMin || brightNeighborCount > neighMax) continue;

                int weight = 255 - g; // 어두울수록 높은 가중치
                sumX += (long)x * weight;
                sumY += (long)y * weight;
                sumW += weight;
            }
        }

        return sumW > 0 ? new Vector2((float)sumX / sumW, (float)sumY / sumW) : (Vector2?)null;
    }

    // 각도 틀어진 밝은 반사 검출
    Vector2? DetectAngledBrightReflection()
    {
        long sumX = 0, sumY = 0, sumW = 0;

        for (int y = 0; y < H; y++)
        {
            int row = y * W;

            for (int x = 0; x < W; x++)
            {
                int idx = row + x;
                byte g = (byte)(ir[idx] >> 8);

                ushort d = depth[idx];
                if (d == 0 || d < minDistanceMM || d > maxDistanceMM) continue;

                // 각도 틀어진 반사: 밝은 중심부이면서 주변과 적당한 대비
                if (g < midRange || g > 250) continue; // 너무 어둡거나 포화된 것 제외

                // 주변 평균과 비교하여 충분히 밝은지 확인
                float avgNeighbor = GetAverageNeighborIntensity(x, y, 2);
                if (avgNeighbor < 50) continue; // 주변이 너무 어두우면 제외

                float contrast = g / (avgNeighbor + 1f);
                if (contrast < 1.2f || contrast > 3.0f) continue; // 적절한 대비 범위

                // 반사 영역의 형태적 특성 확인
                if (!IsReflectivePattern(x, y)) continue;

                int weight = (int)(g * contrast);
                sumX += (long)x * weight;
                sumY += (long)y * weight;
                sumW += weight;
            }
        }

        return sumW > 0 ? new Vector2((float)sumX / sumW, (float)sumY / sumW) : (Vector2?)null;
    }

    // 밝기 점프 기반 검출 (반사 특성 활용)
    Vector2? DetectByIntensityJump()
    {
        long sumX = 0, sumY = 0, sumW = 0;

        for (int y = 1; y < H - 1; y++)
        {
            for (int x = 1; x < W - 1; x++)
            {
                int idx = y * W + x;
                ushort d = depth[idx];
                if (d == 0 || d < minDistanceMM || d > maxDistanceMM) continue;

                byte center = (byte)(ir[idx] >> 8);
                if (center < 60) continue; // 너무 어두운 것은 제외

                // 8방향으로 밝기 차이 검사
                int maxJump = GetMaxIntensityJump(x, y);
                if (maxJump < intensityJumpThreshold) continue;

                // 반사 스티커의 특징: 중심부터 가장자리로 갈수록 급격한 변화
                if (HasReflectiveEdgePattern(x, y))
                {
                    int weight = maxJump;
                    sumX += (long)x * weight;
                    sumY += (long)y * weight;
                    sumW += weight;
                }
            }
        }

        return sumW > 0 ? new Vector2((float)sumX / sumW, (float)sumY / sumW) : (Vector2?)null;
    }

    // 적응형 임계값 검출
    Vector2? DetectByAdaptiveThreshold()
    {
        long sumX = 0, sumY = 0, sumW = 0;

        for (int y = 2; y < H - 2; y++)
        {
            for (int x = 2; x < W - 2; x++)
            {
                int idx = y * W + x;
                ushort d = depth[idx];
                if (d == 0 || d < minDistanceMM || d > maxDistanceMM) continue;

                byte center = (byte)(ir[idx] >> 8);

                // 지역별 적응형 임계값 계산
                float localMean = GetLocalMean(x, y, 3);
                float localStd = GetLocalStandardDeviation(x, y, 3, localMean);

                // 적응형 임계값 = 평균 + k * 표준편차
                float adaptiveThreshold = localMean + 2.0f * localStd;

                // 반사 특성: 지역 평균보다 현저히 다른 값
                bool isSignificant = (center > adaptiveThreshold) ||
                                   (center < localMean - 1.5f * localStd && HasBrightNeighbors(x, y, 2, 4));

                if (isSignificant && IsReflectivePattern(x, y))
                {
                    float deviation = Math.Abs(center - localMean);
                    int weight = (int)(deviation * 2);
                    sumX += (long)x * weight;
                    sumY += (long)y * weight;
                    sumW += weight;
                }
            }
        }

        return sumW > 0 ? new Vector2((float)sumX / sumW, (float)sumY / sumW) : (Vector2?)null;
    }

    // 반사 패턴 확인 (원형 또는 타원형)
    bool IsReflectivePattern(int cx, int cy)
    {
        if (cx < 2 || cx >= W - 2 || cy < 2 || cy >= H - 2) return false;

        byte center = (byte)(ir[cy * W + cx] >> 8);

        // 중심에서 방사형으로 밝기 변화 패턴 확인
        int[] distances = { 1, 2 };
        foreach (int dist in distances)
        {
            float avgIntensity = 0;
            int count = 0;

            // 8방향 체크
            int[] dx = { -dist, -dist, -dist, 0, 0, dist, dist, dist };
            int[] dy = { -dist, 0, dist, -dist, dist, -dist, 0, dist };

            for (int i = 0; i < 8; i++)
            {
                int x = cx + dx[i];
                int y = cy + dy[i];
                if (x >= 0 && x < W && y >= 0 && y < H)
                {
                    avgIntensity += (byte)(ir[y * W + x] >> 8);
                    count++;
                }
            }

            if (count > 0)
            {
                avgIntensity /= count;
                // 중심에서 거리가 멀어질수록 밝기가 급격히 변해야 함
                float expectedChange = dist * 15; // 거리당 예상 변화량
                if (Math.Abs(center - avgIntensity) >= expectedChange)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // 최대 밝기 점프 계산
    int GetMaxIntensityJump(int cx, int cy)
    {
        if (cx < 1 || cx >= W - 1 || cy < 1 || cy >= H - 1) return 0;

        byte center = (byte)(ir[cy * W + cx] >> 8);
        int maxJump = 0;

        // 8방향 체크
        int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

        for (int i = 0; i < 8; i++)
        {
            int x = cx + dx[i];
            int y = cy + dy[i];
            byte neighbor = (byte)(ir[y * W + x] >> 8);
            int jump = Math.Abs(center - neighbor);
            if (jump > maxJump) maxJump = jump;
        }

        return maxJump;
    }

    // 반사 가장자리 패턴 확인
    bool HasReflectiveEdgePattern(int cx, int cy)
    {
        if (cx < 2 || cx >= W - 2 || cy < 2 || cy >= H - 2) return false;

        byte center = (byte)(ir[cy * W + cx] >> 8);

        // 반사 스티커의 특징: 중심에서 가장자리로 급격한 변화
        int radius1Count = 0, radius2Count = 0;
        float radius1Avg = 0, radius2Avg = 0;

        // 반지름 1 체크
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                byte val = (byte)(ir[(cy + dy) * W + (cx + dx)] >> 8);
                radius1Avg += val;
                radius1Count++;
            }
        }
        radius1Avg /= radius1Count;

        // 반지름 2 체크
        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1) continue;
                int x = cx + dx, y = cy + dy;
                if (x >= 0 && x < W && y >= 0 && y < H)
                {
                    byte val = (byte)(ir[y * W + x] >> 8);
                    radius2Avg += val;
                    radius2Count++;
                }
            }
        }
        if (radius2Count > 0) radius2Avg /= radius2Count;

        // 반사 패턴: 중심 -> 1차 링 -> 2차 링으로 갈수록 급격한 변화
        float gradient1 = Math.Abs(center - radius1Avg);
        float gradient2 = Math.Abs(radius1Avg - radius2Avg);

        return (gradient1 > 25 || gradient2 > 20) && (gradient1 + gradient2) > 40;
    }

    // 지역 평균 계산
    float GetLocalMean(int cx, int cy, int radius)
    {
        float sum = 0;
        int count = 0;

        for (int dy = -radius; dy <= radius; dy++)
        {
            int y = cy + dy;
            if (y < 0 || y >= H) continue;

            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = cx + dx;
                if (x < 0 || x >= W) continue;

                sum += (byte)(ir[y * W + x] >> 8);
                count++;
            }
        }

        return count > 0 ? sum / count : 0;
    }

    // 지역 표준편차 계산
    float GetLocalStandardDeviation(int cx, int cy, int radius, float mean)
    {
        float sumSquares = 0;
        int count = 0;

        for (int dy = -radius; dy <= radius; dy++)
        {
            int y = cy + dy;
            if (y < 0 || y >= H) continue;

            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = cx + dx;
                if (x < 0 || x >= W) continue;

                float diff = (byte)(ir[y * W + x] >> 8) - mean;
                sumSquares += diff * diff;
                count++;
            }
        }

        return count > 0 ? (float)Math.Sqrt(sumSquares / count) : 0;
    }

    Vector2? DetectByBlob()
    {
        bool[,] visited = new bool[W, H];
        Vector2? bestBlob = null;
        float bestScore = 0;

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                if (visited[x, y]) continue;

                int idx = y * W + x;
                byte intensity = (byte)(ir[idx] >> 8);
                ushort d = depth[idx];

                if (intensity < midBright || d == 0 || d < minDistanceMM || d > maxDistanceMM) continue;

                var blob = FloodFill(x, y, visited);
                if (blob.Count >= minBlobSize)
                {
                    float avgIntensity = 0;
                    Vector2 center = Vector2.zero;

                    foreach (var point in blob)
                    {
                        int blobIdx = point.y * W + point.x;
                        byte blobIntensity = (byte)(ir[blobIdx] >> 8);
                        avgIntensity += blobIntensity;
                        center += new Vector2(point.x, point.y);
                    }

                    avgIntensity /= blob.Count;
                    center /= blob.Count;

                    float score = avgIntensity * blob.Count;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestBlob = center;
                    }
                }
            }
        }

        return bestBlob;
    }

    List<Vector2Int> FloodFill(int startX, int startY, bool[,] visited)
    {
        List<Vector2Int> blob = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        byte seedIntensity = (byte)(ir[startY * W + startX] >> 8);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            blob.Add(current);

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = current.x + dx;
                    int ny = current.y + dy;

                    if (nx < 0 || nx >= W || ny < 0 || ny >= H || visited[nx, ny]) continue;

                    int nIdx = ny * W + nx;
                    byte nIntensity = (byte)(ir[nIdx] >> 8);

                    if (Math.Abs(nIntensity - seedIntensity) <= 30 && nIntensity >= midBright)
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        return blob;
    }

    Vector2? SelectBestCandidate(List<Vector2> candidates)
    {
        if (candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        // 이전 위치와 가장 가까운 후보 선택
        if (hasValidPosition)
        {
            Vector2 bestCandidate = candidates[0];
            float minDistance = Vector2.Distance(bestCandidate, lastValidPosition);

            for (int i = 1; i < candidates.Count; i++)
            {
                float distance = Vector2.Distance(candidates[i], lastValidPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestCandidate = candidates[i];
                }
            }
            return bestCandidate;
        }

        // 중앙에 가장 가까운 후보 선택
        Vector2 center = new Vector2(W / 2f, H / 2f);
        Vector2 closest = candidates[0];
        float minDist = Vector2.Distance(closest, center);

        for (int i = 1; i < candidates.Count; i++)
        {
            float dist = Vector2.Distance(candidates[i], center);
            if (dist < minDist)
            {
                minDist = dist;
                closest = candidates[i];
            }
        }

        return closest;
    }

    bool IsValidPosition(Vector2 pos)
    {
        if (pos.x < 0 || pos.x >= W || pos.y < 0 || pos.y >= H) return false;

        if (hasValidPosition)
        {
            float distance = Vector2.Distance(pos, lastValidPosition);
            return distance <= maxJumpDistance;
        }

        return true;
    }

    Vector2 ApplySmoothing(Vector2 newPos)
    {
        if (!hasValidPosition) return newPos;

        return Vector2.Lerp(smoothedPosition, newPos, smoothingFactor);
    }

    void AddToHistory(Vector2 pos)
    {
        positionHistory.Enqueue(pos);
        if (positionHistory.Count > historySize)
        {
            positionHistory.Dequeue();
        }
    }

    bool HasBrightNeighbors(int cx, int cy, int r, int need)
    {
        int count = 0;
        for (int dy = -r; dy <= r; dy++)
        {
            int y = cy + dy;
            if (y < 0 || y >= H) continue;
            int row = y * W;

            for (int dx = -r; dx <= r; dx++)
            {
                int x = cx + dx;
                if (x < 0 || x >= W) continue;

                if ((byte)(ir[row + x] >> 8) >= brightHi)
                {
                    count++;
                    if (count >= need) return true;
                }
            }
        }
        return false;
    }

    bool HasGoodContrast(int cx, int cy)
    {
        if (cx < 1 || cx >= W - 1 || cy < 1 || cy >= H - 1) return false;

        int centerIdx = cy * W + cx;
        byte center = (byte)(ir[centerIdx] >> 8);

        float avgNeighbor = GetAverageNeighborIntensity(cx, cy, 1);
        float contrast = center / (avgNeighbor + 1f);

        return contrast >= contrastThreshold * 0.8f; // 약간 낮은 임계값 사용
    }

    float GetAverageNeighborIntensity(int cx, int cy, int radius)
    {
        float sum = 0;
        int count = 0;

        for (int dy = -radius; dy <= radius; dy++)
        {
            int y = cy + dy;
            if (y < 0 || y >= H) continue;
            int row = y * W;

            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx == 0 && dy == 0) continue; // 중심점 제외
                int x = cx + dx;
                if (x < 0 || x >= W) continue;

                sum += (byte)(ir[row + x] >> 8);
                count++;
            }
        }

        return count > 0 ? sum / count : 0;
    }

    int CountBrightNeighbors(int cx, int cy, int r, byte th)
    {
        int count = 0;
        for (int dy = -r; dy <= r; dy++)
        {
            int y = cy + dy;
            if (y < 0 || y >= H) continue;
            int row = y * W;

            for (int dx = -r; dx <= r; dx++)
            {
                int x = cx + dx;
                if (x < 0 || x >= W) continue;
                if ((byte)(ir[row + x] >> 8) >= th) count++;
            }
        }
        return count;
    }

    void DrawCross(Color32[] b, int w, int h, int cx, int cy, int len)
    {
        int yInv = h - 1 - cy;

        // 십자 그리기
        for (int dx = -len; dx <= len; dx++)
        {
            int x = cx + dx;
            if (x >= 0 && x < w) b[yInv * w + x] = new Color32(255, 0, 0, 255);
        }
        for (int dy = -len; dy <= len; dy++)
        {
            int y = cy + dy;
            if (y >= 0 && y < h) b[(h - 1 - y) * w + cx] = new Color32(255, 0, 0, 255);
        }

        // 안정성을 위한 원 그리기
        if (hasValidPosition)
        {
            int radius = 3;
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        int x = cx + dx;
                        int y = cy + dy;
                        if (x >= 0 && x < w && y >= 0 && y < h)
                        {
                            int yPos = h - 1 - y;
                            b[yPos * w + x] = new Color32(0, 255, 0, 128);
                        }
                    }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        CloseKinect();
    }
}