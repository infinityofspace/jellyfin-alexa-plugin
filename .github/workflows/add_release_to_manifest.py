from datetime import datetime
import hashlib
import json
import sys

if len(sys.argv) != 3:
    print("ERROR: Wrong arguments!\nUsage: add_release_to_manifest.py <version> <file>")
    sys.exit(1)

with open("manifest.json", "r") as f:
    manifest = json.load(f)

version = sys.argv[1]

with open(sys.argv[2], "r") as f:
    checksum = hashlib.md5()
    while chunk := f.read(4096):
        checksum.update(chunk)

new_version_info = {
    "version": version,
    "checksum": checksum.hexdigest(),
    "sourceUrl": f"https://github.com/infinityofspace/jellyfin-alexa-plugin/releases/download/{version}/AlexaSkill_{version}.zip",
    "changelog": "See for more details: https://github.com/infinityofspace/jellyfin-alexa-plugin/releases",
    "targetAbi": "10.8.0.0",
    "timestamp": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
}

manifest[0]["versions"].append(new_version_info)

with open("manifest.json", "w") as f:
    json.dump(manifest, f, indent=4)
