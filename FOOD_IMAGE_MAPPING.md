# FoodTasteApp 음식 이미지 연결 복구

## 동작 방식
- 모든 음식 이미지는 `fooddata.FoodNum`을 기준으로 `Assets/Images/FoodImg/{번호}.jpg`, `.jpeg`, `.png`을 순서대로 찾는다.
- 추천(9개 포함), 유사 사용자, 검색, 음식 평가/상세, 인기 차트, 룰렛, 업다운 게임이 `FoodImageCatalog` 공통 로더를 사용한다.
- 기존 추천 데이터의 이름이 같아도 추천 결과의 **원래 FoodNum**을 유지해 이미지가 뒤섞이지 않도록 했다.
- 없는 파일은 다른 음식 사진을 대신 보여주지 않으며, 경고 로그를 남기고 해당 이미지를 비워 둔다.
- Unity Editor에서는 원래 `Assets/Images/FoodImg`에서 로드한다.
- Windows 빌드 후처리기는 이 폴더의 JPG/PNG를 실행 파일 옆의 `FoodImg` 폴더로 복사한다. 빌드된 실행 파일만 별도로 이동하면 이미지가 없어지므로 `FoodImg` 폴더도 함께 배포해야 한다.

## 점검
프로젝트 루트에서 PowerShell 실행:

```powershell
Test-Path '.\Assets\Images\FoodImg\1.jpg'
py -3 .\tools\food_images\check_image_coverage.py
```

원본 XML의 번호와 이미지 번호의 보유 현황을 비교한다. **이 점검은 실제 MySQL 테이블을 조회하지 않는다.** 식품 데이터의 원본 XML 순서가 수정된 경우 `fooddata.FoodNum`과 이미지 번호가 다른지 별도로 확인해야 한다.

## 로컬에 GitHub 변경 적용
- Unity를 종료하고 GitHub `main`을 fetch한 후, 이미지를 사용하는 스크립트와 공통 로더만 최신 버전으로 반영한다.
- 사용자가 로컬 파일을 수정했다면 적용 전에 파일을 백업한다.
- Unity 컴파일 후 추천 음식 화면에서 사진과 이름이 맞는지 확인한다.
- 사진이 보이지 않으면 Editor Console의 `[FoodTasteApp]` 메시지와 점검 결과를 확인한다.

> GitHub 코드 검토와 참조 확인은 완료했지만 실제 사용자 PC의 Unity 실행 검증은 별도이며, 저장소에 없는 이미지 파일은 코드만으로 복구할 수 없다.
