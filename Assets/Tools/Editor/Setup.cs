using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

namespace Tools.Editor
{
    public static class Setup
    {
        [MenuItem("Tools/Setup/Create Default Folders")]
        public static void CreateDefaultFolders() {
            Folders.CreateDefault("_Project", "Animation", "Art", "Materials", "Prefabs", "ScriptableObjects", "Scripts", "Scenes");
            Refresh();
        }

    }
    
    static class Folders {
        public static void CreateDefault(string root, params string[] folders) {
            var fullpath = Combine(Application.dataPath, root);
            if (!Exists(fullpath)) {
                CreateDirectory(fullpath);
            }
            foreach (var folder in folders) {
                CreateSubFolders(fullpath, folder);
            }
        }
    
        private static void CreateSubFolders(string rootPath, string folderHierarchy) {
            var folders = folderHierarchy.Split('/');
            var currentPath = rootPath;
            foreach (var folder in folders) {
                currentPath = Combine(currentPath, folder);
                if (!Exists(currentPath)) {
                    CreateDirectory(currentPath);
                }
            }
        }
    }
}
