﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LDraw
{
    public class LDrawConfig : ScriptableObject
    {
       
        [SerializeField] private string _ModelsPath;
        [SerializeField] private string _ColorConfigPath;
        [SerializeField] private string _MaterialsPath;
        [SerializeField] private string _MeshesPath;
        [SerializeField] private float _Scale;
        [SerializeField] private Material _DefaultOpaqueMaterial;
        [SerializeField] private Material _DefaultTransparentMaterial;

        private string _PartsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/LDraw/parts/";
        private string _SubPartsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/LDraw/parts/s/";
        private string _PrimitivePartsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/LDraw/p/";
        private string _Primitive48PartsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/LDraw/p/48/";
        private Dictionary<string, string> _PrimitiveParts;
        private Dictionary<string, string> _Parts;
        
        //private Dictionary<int, Material> _MainColors;
        private Dictionary<string, Material> _CustomColors;
        //private Dictionary<string, string> _ModelFileNames;
        public Matrix4x4 ScaleMatrix
        {
            get { return Matrix4x4.Scale(new Vector3(_Scale, _Scale, _Scale)); }
        }

        public Material GetColoredMaterial(int code)
        {

            return new Material(_DefaultOpaqueMaterial);
        }
        public Material GetColoredMaterial(string colorString)
        {
            /*if (_CustomColors.ContainsKey(colorString))
                return _CustomColors[colorString];
            var path = _MaterialsPath + colorString + ".mat";
            if (File.Exists(path))
            {
                _CustomColors.Add(colorString, AssetDatabase.LoadAssetAtPath<Material>(path));
            }
            else
            {
                var mat = new Material(_DefaultOpaqueMaterial);

                mat.name = colorString;
                Color color;
                if (ColorUtility.TryParseHtmlString(colorString, out color))
                    mat.color = color;

                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
                _CustomColors.Add(colorString, mat);
            }

            return _CustomColors[colorString];*/
            return new Material(_DefaultOpaqueMaterial);
        }

        public string[] PartFileNames
        {
            get { return _Parts.Keys.ToArray(); }
        }

        


        public string GetSerializedPart(string name)
        {
            
            string serialized;
            if (name.StartsWith("s\\"))
            {
                name = name.Substring(2);
            }
            if (_Parts.ContainsKey(name))
            {
                serialized = File.ReadAllText(_Parts[name]);
            }
            else if(_PrimitiveParts.ContainsKey(name))
            {
                serialized = File.ReadAllText(_PrimitiveParts[name]);
            }
            else
            {
                Debug.Log("Not found: " + name);
                return null;
            }
            return serialized;
        }

        public void InitParts()
        {
            _Parts = new Dictionary<string, string>();
            var files = Directory.GetFiles(_PartsPath, "*.*", SearchOption.AllDirectories);
            

            foreach (var file in files)
            {
                if (!file.Contains(".meta"))
                {
                    string fileName = file.Replace(_PartsPath, "").Split('.')[0];
                    if (!_Parts.ContainsKey(fileName))
                    {
                        _Parts.Add(fileName, file);
                    }
                }
            }
            InitSubParts();
        }

        public void InitSubParts()
        {
            var subFiles = Directory.GetFiles(_SubPartsPath, "*.*", SearchOption.AllDirectories).ToList();

            foreach (var file in subFiles)
            {
                if (!file.Contains(".meta"))
                {
                    
                    string fileName = file.Replace(_SubPartsPath, "").Split('.')[0];
                    
                    if (!_Parts.ContainsKey(fileName))
                    {
                        _Parts.Add(fileName, file);
                    }
                }
            }
        }

        public void InitPrimitiveParts()
        {

            _PrimitiveParts = new Dictionary<string, string>();
            var files = Directory.GetFiles(_PrimitivePartsPath, "*.*", SearchOption.AllDirectories).ToList();
            


            foreach (var filePath in files)
            {
                if (!filePath.Contains(".meta"))
                {
                    string fileName = filePath.Replace(_PrimitivePartsPath, "").Split('.')[0];
                    if (!_PrimitiveParts.ContainsKey(fileName))
                    {
                        _PrimitiveParts.Add(fileName, filePath);
                    }
                    if (fileName.Contains("-")){
                        string OfficialName = File.ReadLines(filePath).ElementAtOrDefault(1);
                        OfficialName = OfficialName.Substring(8);
                        OfficialName = OfficialName.Replace(".dat", "");
                        if (!_PrimitiveParts.ContainsKey(OfficialName))
                        {
                            _PrimitiveParts.Add(OfficialName, filePath);
                        }
                    }
                }
            }
            InitPrimitive48Parts();
        }

        public void InitPrimitive48Parts()
        { 
            var files = Directory.GetFiles(_Primitive48PartsPath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var filePath in files)
            {
                if (!filePath.Contains(".meta"))
                {
                    string fileName = filePath.Replace(_Primitive48PartsPath, "").Split('.')[0];
                    if (!_PrimitiveParts.ContainsKey(fileName))
                    {
                        _PrimitiveParts.Add(fileName, filePath);
                    }
                    if (fileName.Contains("-"))
                    {
                        string OfficialName = File.ReadLines(filePath).ElementAtOrDefault(1);
                        OfficialName = OfficialName.Substring(8);
                        OfficialName = OfficialName.Replace(".dat", "");
                        if (!_PrimitiveParts.ContainsKey(OfficialName))
                        {
                            _PrimitiveParts.Add(OfficialName, filePath);
                        }
                    }


                }
            }
        }

        /*private void ParseColors()
        {
            _MainColors = new Dictionary<int, Material>();
            using (StreamReader reader = new StreamReader(_ColorConfigPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                    line = regex.Replace(line, " ").Trim();
                    var args = line.Split(' ');
                    if (args.Length  > 1 && args[1] == "!COLOUR")
                    {
                        var path =_MaterialsPath + args[2] + ".mat";
                        if (File.Exists(path))
                        {
                            _MainColors.Add(int.Parse(args[4]), AssetDatabase.LoadAssetAtPath<Material>(path));
                        }
                        else
                        {
                            Color color;
                            if (ColorUtility.TryParseHtmlString(args[6], out color))
                            {
                                int alphaIndex = Array.IndexOf(args, "ALPHA");
                                var mat = new Material(alphaIndex  > 0? _DefaultTransparentMaterial : _DefaultOpaqueMaterial);
                                mat.name = args[2];
                                mat.color = alphaIndex > 0? new Color(color.r, color.g, color.b, int.Parse(args[alphaIndex + 1]) / 256f) 
                                    : color;
                            
                                AssetDatabase.CreateAsset(mat, path);
                                _MainColors.Add(int.Parse(args[4]), mat);
                            }
                        }
                    
                    }
                }
                AssetDatabase.SaveAssets();
            }
        }*/
        public static string GetFileName(string[] args, int filenamePos)
        {
            string name = string.Empty;
            for (int i = filenamePos; i < args.Length; i++)
            {
                name += args[i] + ' ';
            }

            return Path.GetFileNameWithoutExtension(name).ToLower();
        }
        public static string GetExtension(string[] args, int filenamePos)
        {
            string name = string.Empty;
            for (int i = filenamePos; i < args.Length; i++)
            {
                name += args[i] + ' ';
            }
         
            return Path.GetExtension(name).Trim();
        }
        private static LDrawConfig _Instance;

        public static LDrawConfig Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = Resources.Load<LDrawConfig>(ConfigPath);
                }

                return _Instance;
            }
        }

        private void OnEnable()
        {
            InitParts();
            InitPrimitiveParts();
        }

        private const string ConfigPath = "Config";
        public const int DefaultMaterialCode = 16;
    }
}