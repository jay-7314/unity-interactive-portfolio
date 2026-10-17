# Unity Interactive Content — Code Samples

전시/체험형 인터랙티브 콘텐츠 16건에서 직접 작성한 Unity(C#) 스크립트를 모아 둔 저장소입니다.
(Unity 2022.3.62f2, UGUI, TextMeshPro)

> 영상·이미지·오디오 등 미디어 에셋, 서드파티 에셋(OpenCV+Unity, Kinect SDK 래퍼, DOTween 등), 내부 IP·키·기기 식별자는 제외/마스킹했습니다.
> 따라서 이 저장소만으로 Unity 프로젝트가 빌드되지는 않으며, **코드 열람용**입니다.

| 폴더 | 핵심 기술 |
|---|---|
| 01-arduino-tcp-interaction | TCP 클라이언트(재접속·핸드셰이크), Arduino 시리얼(백그라운드 스레드 + 큐), JSON 설정 |
| 02-rest-api-quiz-game | REST API 연동(타임아웃·재시도·폴링), Newtonsoft.Json, 오브젝트 풀링 |
| 03-ai-drawing-pipeline | 그림판 텍스처 드로잉, 외부 AI API 업로드→작업→폴링→다운로드, 429 재시도, TCP 이미지 전송 |
| 04-webcam-sound-trigger | WebCamTexture 밝기 샘플링으로 터치 감지, JSON 좌표 설정 |
| 05-webcam-photo-composite | 웹캠 회전/반전, 합성, 타원 크롭 |
| 06-touch-video-map | VideoPlayer 상태 관리, JSON 레이아웃, 원격 로그 전송 |
| 07-arduino-video-quiz | 영상 상태머신 퀴즈, Arduino 입력, 유휴 타이머 |
| 08-mic-record-playback | Microphone 녹음/재생, 영상 동기화 |
| 09~13 | 영상 선택형 키오스크 (공통 템플릿 기반) |
| 14-kinect-ir-tracking | Kinect IR·깊이 프레임(P/Invoke 네이티브 플러그인) 기반 위치 추적, 검출 후보 선택·평활화, JSON 좌표 체류 판정 |
| 15-rfid-card-quiz | RFID 시리얼 입력(자동 재연결), DOTween 카드 뒤집기, 시도 횟수·유휴 타임아웃 |
| 16-dual-display-country-select | 듀얼 디스플레이, JSON 기반 UI 배치, 국가별 현지 시각·시차 계산 |

공통 구조: `GameManager`(싱글톤) · `JsonManager`(StreamingAssets JSON 로드) · `LoadManager`(리소스 로더) · `CoroutineData`.
