// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: GlobalAssemblyInfo.cs  Created: 2015-01-16@17:31
// Last modified: 2015-01-31@23:42 by Tim

using System.Reflection;

[assembly: AssemblyVersion("0.1.2.*")]
[assembly: AssemblyFileVersion("0.1.2.0")]
[assembly: AssemblyInformationalVersion("0.1.2.0-Uncontrolled")]
#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("Tigra Astronomy")]
[assembly: AssemblyProduct("TA.NetMF.MotorControl")]
[assembly: AssemblyCopyright("Copyright © 2014 Tigra Astronomy, all rights reserved")]
[assembly: AssemblyTrademark("Tigra Astronomy")]
[assembly: AssemblyCulture("")]
