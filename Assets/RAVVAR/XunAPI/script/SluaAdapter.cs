using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnceBuild {
    public class SluaAdapter : MonoBehaviour {

        public static SluaAdapter Instance = null;
        public object DoString(string str) {
            return null;
        }
    }
}
namespace SLua{
    public class LuaFunction {
        public object call(params object[] args) {
            return null;        
        }
    }
}