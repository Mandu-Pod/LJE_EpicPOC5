@echo off
setlocal enabledelayedexpansion

REM 배치 파일이 위치한 디렉토리로 이동
cd /d "%~dp0"

REM total 폴더 경로 설정
set "TARGET_DIR=%~dp0total"

REM total 폴더가 없으면 생성
if not exist "%TARGET_DIR%" (
    echo total 폴더를 생성합니다...
    mkdir "%TARGET_DIR%"
)

echo.
echo CS 파일 수집을 시작합니다...
echo 대상 폴더: %TARGET_DIR%
echo.

REM 파일 카운터 초기화
set COUNT=0

REM 현재 디렉토리와 모든 하위 디렉토리에서 .cs 파일 검색 및 복사
for /r %%f in (*.cs) do (
    REM total 폴더 내의 파일은 제외
    echo %%f | findstr /i "%TARGET_DIR%" >nul
    if errorlevel 1 (
        echo 복사 중: %%~nxf
        copy "%%f" "%TARGET_DIR%\" >nul 2>&1
        if !errorlevel! equ 0 (
            set /a COUNT+=1
        ) else (
            echo 경고: %%~nxf 복사 실패
        )
    )
)

echo.
echo ==========================================
echo 복사 완료! 총 %COUNT%개의 파일이 복사되었습니다.
echo 대상 폴더: %TARGET_DIR%
echo ==========================================
echo.

pause