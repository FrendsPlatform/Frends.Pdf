# Frends.PDF.Create
Frends task for creating PDF file.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.PDF/actions/workflows/Create_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.PDF/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.PDF.Create)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.PDF/Frends.PDF.Create|main)

# Requirements

To use the task in Linux agent, you need to install packages `libgdiplus`, `apt-utils` and `libc6-dev`,
since the task uses Windows-based graphics to draw elements to the PDF-file. These packages will emulate
Windows based graphics in Linux. Installing those packages is only availably on on-premises agent.

# Installing

You can install the task via FRENDS UI Task View or you can find the nuget package from the following nuget feed 'Insert nuget feed here'.

# Document Content

Table Field

The Table field accepts JSON to define the structure and content of a table in the PDF.

Simple table example:

{
  "HasHeaderRow": true,
  "TableType": "Table",
  "StyleSettings": {
    "FontFamily": "Times New Roman",
    "FontSizeInPt": 9.0,
    "FontStyle": "Regular",
    "BorderWidthInPt": 0.5,
    "BorderStyle": "All"
  },
  "Columns": [
    {
      "Name": "Name",
      "WidthInCm": 6
    },
    {
      "Name": "Age",
      "WidthInCm": 3
    }
  ],
  "RowData": [
    {
      "Name": "Alice",
      "Age": "30"
    }
  ]
}

Example of table placed in footer with a different column types (Text, Image, Page Number):

{
  "HasHeaderRow": true,
  "TableType": "Footer",
  "StyleSettings": {
    "FontFamily": "Arial",
    "FontSizeInPt": 9.0,
    "FontStyle": "Bold",
    "LineSpacingInPt": 0.0,
    "SpacingBeforeInPt": 0.0,
    "SpacingAfterInPt": 0.0,
    "BorderWidthInPt": 0.5,
    "BorderStyle": "Top"
  },
  "Columns": [
    {
      "Name": "Product",
      "WidthInCm": 5,
      "Type": "Text"
    },
    {
      "Name": "Image",
      "WidthInCm": 5,
      "Type": "Image"
    },
    {
      "Name": "Page Number",
      "WidthInCm": 4,
      "Type": "PageNum"
    }
  ],
  "RowData": [
    {
      "Product": "Laptop",
      "Image": "C:\\images\\laptop.jpg",
      "PageNumber": "{PageNumber}"
    }
  ]
}


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
