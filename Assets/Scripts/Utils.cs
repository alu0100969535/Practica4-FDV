using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace alu0100969535.Utils {
public class Utils {
    public static T Get<T>(GameObject obj, string error) {
        T component = obj.GetComponent<T>();
        if (component == null) {
            Debug.LogError(error);
            throw new System.NullReferenceException(error);
        }
        return component;
    }
}

}


