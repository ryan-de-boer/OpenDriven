// ***********************************************************************
// Copyright (c) 2009 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using NUnit.Engine.Extensibility;

namespace NUnit.Engine
{
    /// <summary>
    /// The ITestListener interface is used to receive notices of significant
    /// events while a test is running. It's single method accepts an Xml string, 
    /// which may represent any event generated by the test framework, the driver
    /// or any of the runners internal to the engine. Use of Xml means that
    /// any driver and framework may add additional events and the engine will
    /// simply pass them on through this interface.
    /// </summary>
    [TypeExtensionPoint(
        Description = "Allows an extension to process progress reports and other events from the test.")]
    public interface ITestEventListener
    {
        /// <summary>
        /// Handle a progress report or other event.
        /// </summary>
        /// <param name="report">An XML progress report.</param>
        void OnTestEvent(string report);
    }
}
