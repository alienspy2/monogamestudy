
import os
import shutil
import subprocess


packerPath = "C:\\Program Files\\7-Zip\\7z.exe"
cwd = os.getcwd()
targetDir = cwd + "\\bin\\Debug\\net8.0-windows"
rootDir = cwd + "\\Assets"

print(cwd)
print(targetDir)
print(rootDir)

# ./assets/ 에 있는 모든 폴더 찾기
def GetAssetFolders():
    return [f for f in os.listdir(rootDir) 
            if os.path.isdir(os.path.join(rootDir, f))]

def Run():
    print(targetDir + "\\PackedAssets")
    if not os.path.exists(targetDir + "\\PackedAssets"):
        os.makedirs(targetDir + "\\PackedAssets")
    folders = GetAssetFolders()
    for folder in folders:
        os.chdir(rootDir + "/" + folder)
        print(f"Folder: {folder}")

        # targetDir + "/PackedAssets/" folder 가 없으면 생성
        dest = targetDir + "\\PackedAssets\\" + folder + ".7z"
        print(dest)
        if os.path.exists(dest):
            os.unlink(dest)
        subprocess.run([packerPath, "a", dest, "*"])

        # dest 의 file 크기 출력
        print(f"File size: {os.path.getsize(dest)} bytes")



if __name__ == "__main__":
    Run()



