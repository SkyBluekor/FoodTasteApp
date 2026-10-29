# FoodTasteApp

Unity로 만든 음식 취향 앱 프로젝트입니다.

## 실행 환경

- Unity 에디터 버전은 `ProjectSettings/ProjectVersion.txt`를 참고하세요.
- 데이터베이스를 사용하는 기능은 로컬 MySQL의 `tasteapp` 데이터베이스에 연결합니다.
- Unity 에디터 또는 실행 파일을 시작하기 전에 환경 변수 `FOODTASTE_DB_PASSWORD`에 MySQL `root` 사용자의 비밀번호를 설정하세요. 비밀번호를 소스 코드나 Git에 저장하지 마세요.

Windows PowerShell에서 현재 세션에 설정하는 예시:

```powershell
$env:FOODTASTE_DB_PASSWORD = '<MySQL 비밀번호>'
```

환경 변수를 설정한 세션에서 Unity를 시작해야 에디터가 해당 값을 사용할 수 있습니다. 기존에 실행 중인 Unity 에디터는 설정 후 다시 시작하세요.
