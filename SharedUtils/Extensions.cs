#region License

// MIT License
//
// Copyright (c) 2018 Marcus Technical Services, Inc. http://www.marcusts.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion License

namespace SharedForms.Common.Utils
{
   #region Imports

   using System;
   using System.Collections.Concurrent;
   using System.Collections.Generic;
   using System.Linq;
   using System.Reflection;

   #endregion Imports

   public static class Extensions
   {
     #region Private Variables

     private const double NUMERIC_ERROR = 0.001;

     #endregion Private Variables

     #region Public Properties

     public static bool? EmptyNullableBool => new bool?();

     #endregion Public Properties

     //public static readonly Random GLOBAL_RANDOM = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

     //private const double NUMERIC_ERROR = 0.001;

     //public const string TRUE_STR = "true";

     //public const string FALSE_STR = "false";

     //public static bool? EmptyNullableBool => new bool?();

     //public static bool IsSameAs(this double mainD, double otherD)
     //{
     //   return Math.Abs(mainD - otherD) < NUMERIC_ERROR;
     //}

     //public static bool IsDifferentThan(this double mainD, double otherD)
     //{
     //   return !IsSameAs(mainD, otherD);
     //}

     //public static bool IsEmpty(this double mainD)
     //{
     //   return IsSameAs(mainD, 0);
     //}

     //public static bool IsNotEmpty(this double mainD)
     //{
     //   return !IsEmpty(mainD);
     //}

     //public static bool IsSameAs(this float mainF, float otherF)
     //{
     //   return Math.Abs(mainF - otherF) < NUMERIC_ERROR;
     //}

     //public static bool IsDifferentThan(this float mainF, float otherF)
     //{
     //   return !IsSameAs(mainF, otherF);
     //}

     //public static bool IsEmpty(this string str)
     //{
     //   return String.IsNullOrWhiteSpace(str);
     //}

     //public static bool IsNotEmpty(this string str)
     //{
     //   return !IsEmpty(str);
     //}

     //public static bool IsEmpty<T>(this IEnumerable<T> list)
     //{
     //   return list == null || !list.Any();
     //}

     //public static bool IsNotEmpty<T>(this IEnumerable<T> list)
     //{
     //   return !IsEmpty(list);
     //}

     //public static bool IsSameAs(this string mainStr, string otherStr)
     //{
     //   return String.Compare(mainStr, otherStr, StringComparison.CurrentCultureIgnoreCase) == 0;
     //}

     //public static bool IsDifferentThan(this string mainStr, string otherStr)
     //{
     //   return !IsSameAs(mainStr, otherStr);
     //}

     //public static int RoundToInt(this double floatVal)
     //{
     //   return (int) Math.Round(floatVal, 0);
     //}

     //public static bool IsLessThanOrEqualTo(this double thisD, double otherD)
     //{
     //   return IsSameAs(thisD, otherD) || thisD < otherD;
     //}

     //public static bool IsGreaterThanOrEqualTo(this double thisD, double otherD)
     //{
     //   return IsSameAs(thisD, otherD) || thisD > otherD;
     //}

     //public static bool IsTrue(this bool? b)
     //{
     //   return b.HasValue && b.Value;
     //}

     //public static bool IsNotTheSame(this bool? first, bool? second)
     //{
     //   return first == null != (second == null)
     //        ||
     //        IsNotAnEqualObjectTo(first, second);
     //}

     //public static bool IsNonNullRegexMatch(this string s, string regex)
     //{
     //   return s != null && Regex.IsMatch(s, regex, RegexOptions.IgnoreCase);
     //}

     #region Public Methods

     public static void AddOrUpdate<T, U>(this ConcurrentDictionary<T, U> retDict, T key, U value)
     {
       retDict.AddOrUpdate(key, value, (k, v) => v);
     }

     /// <summary>
     /// Determines if two collections of properties contain the same actual values. Can be called
     /// for a single property using the optional parameter.
     /// </summary>
     /// <typeparam name="T">The type of class which is being evaluated for changes.</typeparam>
     /// <param name="mainViewModel">The main class for comparison.</param>
     /// <param name="otherViewModel">The secondary class for comparison.</param>
     /// <param name="singlePropertyName">If set, this will be the only property interrogated.</param>
     /// <param name="propInfos">The property info collection that will be analyzed for changes.</param>
     /// <returns></returns>
     public static bool AnySettablePropertyHasChanged<T>(this T mainViewModel, T otherViewModel,
       string singlePropertyName = default(string), params PropertyInfo[] propInfos)
     {
       var isChanged = false;

       foreach (var propInfo in propInfos)
       {
         // If we have a single property, only allow that one through.
         if (singlePropertyName.IsNotEmpty<char>() && propInfo.Name.IsDifferentThan(singlePropertyName))
         {
            continue;
         }

         var currentValue = propInfo.GetValue(mainViewModel);
         var otherValue = propInfo.GetValue(otherViewModel);

         if (currentValue.IsNotAnEqualObjectTo(otherValue))
         {
            isChanged = true;
            break;
         }
       }

       return isChanged;
     }

     public static string CleanUpServiceErrorText(this string errorText)
     {
       // Find the LAST colon in the string
       var colonPos = errorText.LastIndexOf(":", StringComparison.CurrentCultureIgnoreCase);
       if (colonPos > 0)
       {
         var newErrorText = errorText.Substring(colonPos + 1);
         newErrorText = newErrorText.Trim('{');
         newErrorText = newErrorText.Trim('}');
         newErrorText = newErrorText.Trim('"');

         return newErrorText;
       }

       return String.Empty;
     }

     //#if AUDIT_PROP_INFO
     //            Debug.WriteLine("SUCCESS: copied prop info ->" + propInfo.Name + "<-");
     //#endif
     //         }
     //       }
     //       catch (Exception ex)
     //       {
     //         Debug.WriteLine("CopySettablePropertyValuesFrom error ->" + ex.Message + "<-");
     //       }
     //     }
     public static PropertyInfo[] GetAllPropInfos<T>()
     {
       var retInfos = typeof(T).GetRuntimeWriteableProperties();
       return retInfos.ToArray();
     }

     public static int GetEnumCount<T>()
     {
       if (!typeof(T).IsEnum)
       {
         throw new Exception("Not enum");
       }

       return Enum.GetNames(typeof(T)).Length;
     }

     /// <summary>
     /// Gets the properties for a type that have a public setter.
     /// </summary>
     /// <param name="type">The type being analyzed.</param>
     /// <returns>The public settable property info's for this type.</returns>
     /// <remarks>This method is NOT THREAD SAFE due to the list add.</remarks>
     public static PropertyInfo[] GetRuntimeWriteableProperties(this Type type)
     {
       if (!type.IsInterface)
       {
         return type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
       }

       // ELSE
       var propertyInfos = new List<PropertyInfo>();

       var considered = new List<Type>();
       var queue = new Queue<Type>();
       considered.Add(type);
       queue.Enqueue(type);
       while (queue.Count > 0)
       {
         var subType = queue.Dequeue();
         foreach (var subInterface in subType.GetInterfaces())
         {
            if (considered.Contains(subInterface))
            {
              continue;
            }

            considered.Add(subInterface);
            queue.Enqueue(subInterface);
         }

         var typeProperties = subType.GetProperties(
            BindingFlags.FlattenHierarchy
            | BindingFlags.Public
            | BindingFlags.Instance);

         var newPropertyInfos = typeProperties
            .Where(x => !propertyInfos.Contains(x));

         propertyInfos.InsertRange(0, newPropertyInfos);
       }

       return propertyInfos.ToArray();
     }

     //public static bool IsAnEqualReferenceTo<T>(this T mainObj, T compareObj)
     //   where T : class
     //{
     //   return
     //     (mainObj == null && compareObj == null)
     //     ||
     //     (
     //       ((mainObj == null) == (compareObj == null))
     //       &&
     //       (ReferenceEquals(compareObj, mainObj))
     //     );
     //}

     //public static bool IsNotAnEqualReferenceTo(this object mainObj, object compareObj)
     //{
     //   return !IsAnEqualReferenceTo(mainObj, compareObj);
     //}

     //public static bool IsAnEqualObjectTo(this object mainObj, object compareObj)
     //{
     //   return
     //     mainObj == null && compareObj == null
     //     ||
     //     mainObj != null && mainObj.Equals(compareObj)
     //     ||
     //     compareObj != null && compareObj.Equals(mainObj);
     //}

     //public static bool IsNotAnEqualObjectTo(this object mainObj, object compareObj)
     //{
     //   return !IsAnEqualObjectTo(mainObj, compareObj);
     //}

     // public static PropertyInfo[] CleanPropInfos(this PropertyInfo[] propInfos, params string[]
     // namesToRemove) { if (IsEmpty(namesToRemove)) { return propInfos; }

     // var retPropInfoList = new List<PropertyInfo>();

     // foreach (var propInfo in propInfos) { var foundPropInfo = namesToRemove.FirstOrDefault(n =>
     // IsSameAs(n, propInfo.Name));

     // if (foundPropInfo == null) { retPropInfoList.Add(propInfo); } }

     // return retPropInfoList.ToArray(); }

     // /// <summary> /// Copy the values from the specified properties from value to target. ///
     // </summary> /// <typeparam name="T">*Unused* -- required for referencing only.</typeparam>
     // /// <param name="targetViewModel">The view model to copy *to*.</param> /// <param
     // name="valueViewModel">The view model to copy *from*.</param> /// <param name="propInfos">The
     // property info records to use to get and set values.</param> public static void
     // CopySettablePropertyValuesFrom<T>(this T targetViewModel, T valueViewModel, params
     // PropertyInfo[] propInfos) { if (propInfos == null || !propInfos.Any()) { propInfos =
     // typeof(T).GetRuntimeWriteableProperties(); }

     //       try
     //       {
     //         foreach (var propInfo in propInfos)
     //         {
     //#if AUDIT_PROP_INFO
     //            Debug.WriteLine("  About to copy prop info ->" + propInfo.Name + "<-");
     //#endif

     //#if MANAGE_MODES_COPY
     //            if (propInfo.PropertyType == typeof(Modes))
     //            {
     //              var mode = propInfo.GetValue(valueViewModel);
     //              propInfo.SetValue(targetViewModel, mode);
     //            }
     //            else
     //            {
     //#endif
     //              propInfo.SetValue(targetViewModel, propInfo.GetValue(valueViewModel));
     //#if MANAGE_MODES_COPY
     //            }
     //#endif
     //public static void ClearWriteableProperties<T>(this T target)
     //   where T : new()
     //{
     //   var propInfos = typeof(T).GetRuntimeWriteableProperties().ToArray();

     // var defaultT = Activator.CreateInstance<T>();

     //   if (IsNotEmpty(propInfos))
     //   {
     //     target.CopySettablePropertyValuesFrom(defaultT, propInfos);
     //   }
     //}

     //public static bool ToBool(this string boolStr)
     //{
     //   return IsSameAs(boolStr, TRUE_STR);
     //}

     //public static string ToBoolStr(this bool b)
     //{
     //   return b ? TRUE_STR : FALSE_STR;
     //}

     public static string GetStringFromObject(object value)
     {
       if (value is string s)
       {
         return s.IsEmpty<char>() ? String.Empty : s;
       }

       return value == null ? String.Empty : value.ToString();
     }

     public static bool HasNoValue(this double? db)
     {
       return !db.HasValue;
     }

     public static bool IsDifferentThan(this DateTime mainDateTime, DateTime otherDateTime)
     {
       return !mainDateTime.IsSameAs(otherDateTime);
     }

     public static bool IsEmpty(this DateTime mainDateTime)
     {
       return mainDateTime.IsSameAs(default(DateTime));
     }

     public static bool IsTypeOrAssignableFromType(this Type mainType, Type targetType)
     {
       return mainType == targetType || targetType.IsAssignableFrom(mainType);
     }

     public static bool IsNotEmpty(this DateTime mainDateTime)
     {
       return !mainDateTime.IsEmpty();
     }

     public static bool IsSameAs(this DateTime mainDateTime, DateTime otherDateTime)
     {
       return mainDateTime.CompareTo(otherDateTime) == 0;
     }

     public static double MinOfTwoDoubles(double width, double height)
     {
       return Math.Min(Convert.ToSingle(width), Convert.ToSingle(height));
     }

     public static int ToRoundedInt(this double d)
     {
       return (int) Math.Round(d, 0);
     }

     public static bool IsEmpty<T>(this IEnumerable<T> list)
     {
       return list == null || !list.Any();
     }

     public static bool IsNotEmpty<T>(this IEnumerable<T> list)
     {
       return !list.IsEmpty();
     }

     public static bool IsAnEqualObjectTo(this object mainObj, object compareObj)
     {
       return
         mainObj == null && compareObj == null
         ||
         mainObj != null && mainObj.Equals(compareObj)
         ||
         compareObj != null && compareObj.Equals(mainObj);
     }

     public static bool IsAnEqualReferenceTo<T>(this T mainObj, T compareObj)
       where T : class
     {
       return
         mainObj == null && compareObj == null
         ||
         mainObj == null == (compareObj == null)
         && ReferenceEquals(compareObj, mainObj);
     }

     public static bool IsDifferentThan(this double mainD, double otherD)
     {
       return !mainD.IsSameAs(otherD);
     }

     public static bool IsDifferentThan(this float mainF, float otherF)
     {
       return !mainF.IsSameAs(otherF);
     }

     public static bool IsDifferentThan(this string mainStr, string otherStr)
     {
       return !mainStr.IsSameAs(otherStr);
     }

     public static bool IsEmpty(this double mainD)
     {
       return mainD.IsSameAs(0);
     }

     public static bool IsEmpty(this string str)
     {
       return String.IsNullOrWhiteSpace(str);
     }

     //public static bool IsEmpty<T>(this IEnumerable<T> list)
     //{
     //   return list == null || !list.Any();
     //}

     public static bool IsGreaterThanOrEqualTo(this double thisD, double otherD)
     {
       return thisD.IsSameAs(otherD) || thisD > otherD;
     }

     public static bool IsLessThanOrEqualTo(this double thisD, double otherD)
     {
       return thisD.IsSameAs(otherD) || thisD < otherD;
     }

     public static bool IsNotAnEqualObjectTo(this object mainObj, object compareObj)
     {
       return !mainObj.IsAnEqualObjectTo(compareObj);
     }

     public static bool IsNotAnEqualReferenceTo<T>(this T mainObj, T compareObj)
       where T : class
     {
       return !mainObj.IsAnEqualReferenceTo(compareObj);
     }

     public static bool IsNotEmpty(this double mainD)
     {
       return !mainD.IsEmpty();
     }

     public static bool IsNotEmpty(this string str)
     {
       return !str.IsEmpty();
     }

     //public static bool IsNotEmpty<T>(this IEnumerable<T> list)
     //{
     //   return !list.IsEmpty();
     //}

     public static bool IsNotTheSame(this bool? first, bool? second)
     {
       return first == null != (second == null)
            ||
            first.IsNotAnEqualObjectTo(second);
     }

     public static bool IsSameAs(this double mainD, double otherD)
     {
       return Math.Abs(mainD - otherD) < NUMERIC_ERROR;
     }

     public static bool IsSameAs(this float mainF, float otherF)
     {
       return Math.Abs(mainF - otherF) < NUMERIC_ERROR;
     }

     public static bool IsSameAs(this string mainStr, string otherStr)
     {
       return String.Compare(mainStr, otherStr, StringComparison.CurrentCultureIgnoreCase) == 0;
     }

     public static bool IsTrue(this bool? b)
     {
       return b.HasValue && b.Value;
     }

     public static int RoundToInt(this double floatVal)
     {
       return (int)Math.Round(floatVal, 0);
     }


     /// <summary>
     /// Returns the value for a key, if that key exists in the dictionary.
     /// </summary>
     /// <remarks>This is *not* thread safe</remarks>
     public static string TryToGetKeyValue(this IDictionary<string, string> dict, string key)
     {
       if (dict != null && dict.ContainsKey(key))
       {
         return dict[key];
       }

       return String.Empty;
     }

     #endregion Public Methods

     //public static T GetRandomEnum<T>()
     //{
     //   Debug.Assert(typeof(T).IsEnum, "Must pass an enum to GetRandomEnum");

     // var rand = GLOBAL_RANDOM.Next(GetEnumCount<T>());

     // if (Enum.GetValues(typeof(T)) is T[] allValues) { return allValues[rand]; }

     //   return default(T);
     //}
   }
}
