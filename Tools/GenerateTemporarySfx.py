import math  # 파형 계산
import struct  # 정수 표본 변환
import wave  # 웨이브 파일 저장
from pathlib import Path  # 경로 처리

RATE = 22050  # 초당 표본 수
DESTINATION = Path(__file__).resolve().parents[1] / "Assets" / "Audio" / "SFX" / "Resources" / "Audio"  # 임시 효과음 폴더
SOUNDS = [  # 임시 효과음 설정
    ("SFX_Turret_Role_Melee.wav", 0.075, 680.0, 390.0, 0.16),  # 짧은 공격음
    ("SFX_Player_Hit.wav", 0.18, 240.0, 100.0, 0.24),  # 플레이어 피격음
    ("SFX_Player_BodyBreak.wav", 0.34, 180.0, 55.0, 0.25),  # 몸통 손실음
    ("SFX_UI_LevelUp.wav", 0.34, 440.0, 880.0, 0.20),  # 레벨업 상승음
    ("SFX_UI_Merge.wav", 0.28, 300.0, 620.0, 0.20),  # 합성 상승음
    ("SFX_Elite_Appear.wav", 0.50, 250.0, 390.0, 0.22),  # 엘리트 출현 알림
    ("SFX_Boss_Appear.wav", 0.72, 130.0, 190.0, 0.24),  # 보스 출현 알림
]  # 효과음 목록 종료

def write_sound(name, duration, first, last, amplitude):  # 임시 파형 생성
    count = int(RATE * duration)  # 전체 표본 수
    phase = 0.0  # 현재 파형 위상
    frames = bytearray()  # 출력 표본 버퍼
    for index in range(count):  # 표본 순회
        progress = index / max(1, count - 1)  # 전체 시간 비율
        frequency = first + (last - first) * progress  # 현재 음높이
        phase += 2.0 * math.pi * frequency / RATE  # 다음 위상
        attack = min(1.0, index / max(1.0, RATE * 0.012))  # 시작 경계 완화
        release = (1.0 - progress) ** 1.6  # 끝 경계 완화
        sample = amplitude * attack * release * (math.sin(phase) + 0.18 * math.sin(phase * 2.0)) / 1.18  # 부드러운 합성 파형
        frames.extend(struct.pack("<h", int(max(-1.0, min(1.0, sample)) * 32767)))  # 단일 표본 기록
    with wave.open(str(DESTINATION / name), "wb") as output:  # 웨이브 파일 생성
        output.setnchannels(1)  # 단일 채널
        output.setsampwidth(2)  # 16비트 표본
        output.setframerate(RATE)  # 표본 속도
        output.writeframes(frames)  # 전체 파형 저장

DESTINATION.mkdir(parents=True, exist_ok=True)  # 출력 폴더 준비
for sound in SOUNDS:  # 모든 효과음 순회
    write_sound(*sound)  # 개별 효과음 생성
