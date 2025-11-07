import os
from pathlib import Path

dir_path = Path('.')

terms_input = input("Gib die Liste der Begriffe ein (komma-separiert, z. B. Anvil,Backpack,Battery): ").strip()
if not terms_input:
    print("Keine Begriffe eingegeben – Abbruch.")
    exit()

terms = [t.strip() for t in terms_input.split(',') if t.strip()]

image_exts = {'.png', '.jpg', '.jpeg', '.gif', '.bmp', '.webp', '.svg', '.tiff', '.ico', '.avif', '.heic'}

renamed_count = 0
found_terms = set()
multiple_warning = []

print("\nStarte Umbenennung...\n")

for term in terms:
    lower_term = term.lower()
    term_matched = False
    match_count = 0
    
    for file_path in dir_path.iterdir():
        if not file_path.is_file():
            continue
            
        ext = file_path.suffix.lower()
        if ext not in image_exts:
            continue
            
        stem = file_path.stem
        lower_stem = stem.lower()
        
        if lower_term in lower_stem:
            match_count += 1
            term_matched = True
            
            new_ext = ext
            new_name = lower_term + new_ext
            new_path = file_path.with_name(new_name)
            
            if new_path.exists():
                print(f"Überspringe {file_path.name} → {new_name} (Zieldatei existiert bereits)")
                continue
            
            try:
                file_path.rename(new_path)
                print(f"Umbenannt: {file_path.name} → {new_name}")
                renamed_count += 1
            except Exception as e:
                print(f"Fehler beim Umbenennen von {file_path.name}: {e}")
    
    if term_matched:
        found_terms.add(lower_term)
        if match_count > 1:
            multiple_warning.append(f"{term} ({match_count} Dateien → nur eine umbenannt)")

missing_terms = [term for term in terms if term.lower() not in found_terms]

print(f"\nFertig! {renamed_count} Datei(en) umbenannt.")

if multiple_warning:
    print("\nMehrere Dateien pro Begriff gefunden (nur jeweils eine umbenannt):")
    for w in multiple_warning:
        print(f"   - {w}")

if missing_terms:
    print("\nKeine Bilder gefunden für folgende Begriffe:")
    for m in missing_terms:
        print(f"   - {m}")
else:
    print("\n✓ Alle Begriffe hatten mindestens ein passendes Bild!")
