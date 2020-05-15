using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringUtil
{
    static readonly StringBuilder sb = new StringBuilder();

    static readonly object lockObj = new object();

    public static string Contact(params object[] objs)
    {
        lock (lockObj)
        {
            sb.Remove(0, sb.Length);
            for (int i = 0; i < objs.Length; i++)
            {
                if (null != objs[i])
                {
                    sb.Append(objs[i]);
                }
            }
            return sb.ToString();
        }
    }
}
