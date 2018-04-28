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

namespace SharedForms.Views.Controls
{
   #region Imports

   using Common.Utils;
   using System;
   using System.Collections;
   using System.Collections.Generic;

   #endregion Imports

   public class FlexibleStack<T> : IEnumerable<T>
   {
      #region Private Variables

      private readonly IList<T> _items = new List<T>();

      #endregion Private Variables

      #region Private Methods

      private T PopOrPeek(bool removeIt)
      {
         if (_items.Count > 0)
         {
            var temp = _items[_items.Count - 1];

            if (removeIt)
            {
               _items.RemoveAt(_items.Count - 1);
            }

            return temp;
         }

         // FAIL CASE
         return default(T);
      }

      #endregion Private Methods

      #region Public Methods

      public void Clear()
      {
         _items.Clear();
      }

      public IEnumerator<T> GetEnumerator()
      {
         return _items.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public T Peek()
      {
         return PopOrPeek(false);
      }

      public T Pop()
      {
         return PopOrPeek(true);
      }

      public void Push(T item)
      {
         _items.Add(item);
      }

      public void RemoveIfPresent(T item, Predicate<T> dupTest)
      {
         if (_items.IsEmpty() || dupTest == null)
         {
            return;
         }

         var itemIdx = 0;

         do
         {
            var currItem = _items[itemIdx];

            if (dupTest(currItem))
            {
               _items.Remove(item);

               // Do *not* increment item idx
            }
            else
            {
               itemIdx++;
            }
         } while (itemIdx < _items.Count);
      }

      #endregion Public Methods

      //public void RemoveIfPresent(T item)
      //{
      //   // Can remove more than one.
      //   while (_items.Contains(item))
      //   {
      //      _items.Remove(item);
      //   }
      //}
   }
}
