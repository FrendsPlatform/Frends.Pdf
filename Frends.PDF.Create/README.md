# Frends.PDF.Create
Frends task for creating PDF file.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.PDF/actions/workflows/Create_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.PDF/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.PDF.Create)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.PDF/Frends.PDF.Create|main)

# Requirements

To use the task in Linux agent, you need to install packages `libgdiplus`, `apt-utils` and `libc6-dev`,
since the task uses Windows-based graphics to draw elements to the PDF-file. These packages will emulate
Windows based graphics in Linux. Installing those packages is only available on on-premises agent.

# Installing

You can install the task via FRENDS UI Task View or you can find the nuget package from the following nuget feed 'Insert nuget feed here'.

# Building

Clone a copy of the repo.

`git clone https://github.com/FrendsPlatform/Frends.PDF`

Go to the task directory.

`cd Frends.PDF/Frends.PDF.Create`

Build the solution.

`dotnet build`

Run tests.

`dotnet test`

Create a nuget package.

`dotnet pack --configuration Release`
