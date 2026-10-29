@echo off
setlocal
cd /d "%~dp0"
where py >nul 2>&1
if %errorlevel%==0 (
  py -3 "tools\db\install_demo_db.py"
) else (
  where python >nul 2>&1
  if %errorlevel%==0 (
    python "tools\db\install_demo_db.py"
  ) else (
    echo [ERROR] Python 3 not found. Install Python 3 and try again.
    pause
    exit /b 1
  )
)
if errorlevel 1 (
  echo.
  echo Database setup failed. Check the message above.
) else (
  echo.
  echo Database setup finished. Read DATABASE_SETUP.md for Unity setup.
)
pause
