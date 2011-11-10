using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace DiversityPhone.Utility
{
    public static class ReflectionOperations
    {
        public static void copyAllFields(Object from, Object to)
        {
            if (!from.GetType().Equals(to.GetType()))
                throw new ArgumentException("Copy Operation needs two Objects of the same type.");
            IList<PropertyInfo> piList = ReflectionOperations.RetrieveAllProperties(from.GetType());
            foreach (PropertyInfo pi in piList)
            {
                Object val = pi.GetValue(from, null);
                pi.SetValue(to, val, null);
            }
        }

        public static IList<FieldInfo> RetrieveAllFields(Type t)
        {
            IList<FieldInfo> tmp = new List<FieldInfo>();

            if (t == typeof(Object))
            {
                return tmp;
            }

            foreach (FieldInfo fi in t.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                tmp.Add(fi);
            }

            Type recursiveType = t.BaseType;
            foreach (FieldInfo fi in RetrieveAllFields(recursiveType))
            {
                tmp.Add(fi);
            }
            return tmp;
        }


        public static IList<PropertyInfo> RetrieveAllProperties(Type t)
        {
            IList<PropertyInfo> tmp = new List<PropertyInfo>();

            if (t == typeof(Object))
            {
                return tmp;
            }

            foreach (PropertyInfo pi in t.GetProperties())
            {
                tmp.Add(pi);
            }

            Type recursiveType = t.BaseType;
            foreach (PropertyInfo pi in RetrieveAllProperties(recursiveType))
            {
                tmp.Add(pi);
            }
            return tmp;
        }

    }
}
