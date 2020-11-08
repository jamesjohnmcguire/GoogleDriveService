/////////////////////////////////////////////////////////////////////////////
// <copyright file="GlobalSuppressions.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
//
// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
/////////////////////////////////////////////////////////////////////////////

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Do not agree with the rule.", Scope = "member", Target = "~M:BackupManagerLibrary.Account.Authenticate~System.Boolean")]
[assembly: SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Do not agree with the rule.", Scope = "member", Target = "~M:BackupManagerLibrary.GoogleDrive.CreateFolder(System.String,System.String)~Google.Apis.Drive.v3.Data.File")]
[assembly: SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Do not agree with the rule.", Scope = "member", Target = "~M:BackupManagerLibrary.GoogleDrive.Upload(System.String,System.String,System.String,System.Boolean)")]
