import pathlib
import re
import unittest


ROOT = pathlib.Path(__file__).resolve().parents[2]
CHARACTERS = ROOT / "Assets" / "Data" / "Characters"
SPRITES = ROOT / "Assets" / "Art" / "Sprites"
NAMES = ("Balanced", "Mobile", "Heavy", "Fusion", "Holy", "Dark")


class CharacterSkinAssetsTests(unittest.TestCase):
    def test_six_profiles_reference_existing_sprites_and_distinct_colors(self):
        known_guids = set()
        for meta in SPRITES.rglob("*.png.meta"):
            match = re.search(r"^guid: ([0-9a-f]{32})$", meta.read_text(encoding="utf-8"), re.MULTILINE)
            if match and meta.with_suffix("").is_file():
                known_guids.add(match.group(1))

        colors = set()
        for name in NAMES:
            text = (CHARACTERS / f"Prototype{name}.asset").read_text(encoding="utf-8")
            for field in ("battleHeadSprite", "battleBodySprite", "battleTailSprite"):
                match = re.search(rf"^  {field}: \{{fileID: 21300000, guid: ([0-9a-f]{{32}}), type: 3\}}$", text, re.MULTILINE)
                self.assertIsNotNone(match, f"{name}: {field} is missing")
                self.assertIn(match.group(1), known_guids, f"{name}: {field} does not reference an existing sprite")
            tint = re.search(r"^  battleTint: \{r: ([\d.]+), g: ([\d.]+), b: ([\d.]+), a: ([\d.]+)\}$", text, re.MULTILINE)
            self.assertIsNotNone(tint, f"{name}: battle tint is missing")
            color = tuple(float(channel) for channel in tint.groups())
            self.assertEqual(color[3], 1.0, f"{name}: skin is not fully visible")
            self.assertNotIn(color, colors, f"{name}: battle tint is duplicated")
            colors.add(color)


if __name__ == "__main__":
    unittest.main()
