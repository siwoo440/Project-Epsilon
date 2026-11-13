import math  # 합성 파형 계산
import struct  # 음원 바이트 변환
import wave  # 임시 웨이브 저장
from pathlib import Path  # 실제 저장 경로

rate = 11025  # 임시 음원 표본 속도
length = 8  # 반복 음원 길이
folder = Path("Assets/Audio/BGM/Resources/Music")  # 교체 가능한 음원 위치
folder.mkdir(parents=True, exist_ok=True)  # 음원 폴더 준비
tracks = {  # 장면별 임시 음계
    "BGM_Menu": (110, 165, 220, 165),  # 메뉴 음계
    "BGM_Plain": (131, 165, 196, 165),  # 평원 음계
    "BGM_Ruins": (110, 131, 165, 147),  # 폐허 음계
    "BGM_Swamp": (98, 123, 147, 131),  # 습지 음계
    "BGM_Boss": (147, 175, 196, 165),  # 보스 음계
}  # 임시 음계 종료
for name, notes in tracks.items():  # 각 장면 음원 생성
    with wave.open(str(folder / (name + ".wav")), "wb") as output:  # 실제 웨이브 파일
        output.setnchannels(1)  # 단일 채널 설정
        output.setsampwidth(2)  # 16비트 샘플 설정
        output.setframerate(rate)  # 표본 속도 적용
        for index in range(rate * length):  # 전체 음원 표본 순회
            seconds = index / rate  # 현재 재생 시각
            position = seconds % 2  # 개별 음 길이
            envelope = math.sin(math.pi * position / 2) ** 2  # 음 연결 경계 감쇠
            note = notes[int(seconds // 2) % len(notes)]  # 현재 장면 음계
            sample = envelope * (math.sin(2 * math.pi * note * seconds) + 0.25 * math.sin(2 * math.pi * note * 2 * seconds)) * 0.13  # 낮은 음량의 임시 파형
            output.writeframesraw(struct.pack("<h", int(sample * 32767)))  # 실제 음원 표본 기록
