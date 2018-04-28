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

namespace SharedForms.Common.Interfaces
{
   #region Imports

   using System;

   #endregion Imports

   public interface IProvidePageEvents
   {
      #region Public Properties

      /// <summary>
      /// Regrettable use of object; we could type-cast, but than makes it difficult to pass
      /// IProvidePageEvents at lower levels without omniscient knowledge off the parent page type.
      /// </summary>
      /// <remarks>
      /// The function is better than a property when there is a chance of nesting to view inside
      /// view, etc. Whenever this property is assigned, it will always seek a legal value.
      /// Otherwise, an assignment might begin with null ands then never change. The root of these
      /// events is a known, valid page that should be seekable by any deriver or nested deriver.
      /// </remarks>
      Func<object> GetEventBroadcaster { get; }

      #endregion Public Properties
   }
}
