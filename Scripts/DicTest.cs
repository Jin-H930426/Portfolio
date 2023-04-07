using System.Collections;
using System.Collections.Generic;
using JH;
using UnityEngine;

public class DicTest : MonoBehaviour
{
    [System.Serializable]
    public struct MyStruct
    {
        public int a;
        public int b;
    }
    [System.Serializable]
    public enum MyEnum
    {
        A,
        B,
        C,
    }
    public SerializedDictionary<string, string> strDic  = new();
    public SerializedDictionary<int, string> intDic = new();
    public SerializedDictionary<MyEnum, string> enumDic = new();
    public SerializedDictionary<MyStruct, string> structDic = new();
    public SerializedDictionary<string, MyStruct> structDic2 = new();
    public SerializedDictionary<MyStruct, MyStruct> structDic3 = new();
    public SerializedDictionary<string, int[]> stringArrayDic = new();
    public SerializedDictionary<string, List<MyStruct>> stringListDic = new();
}
