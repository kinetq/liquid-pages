# Kinetq.LiquidPages.Extension

A Visual Studio extension that provides enhanced support for Liquid template files in your .NET projects. This extension is meant to work 
specifically with [Kinetq.LiquidPages](https://kinetq.com/docs/open-source-software/liquid-pages), a framework for building web applications using Liquid templates in .NET and emulates the RazorPages patterns.

## Features

### 🎨 Syntax Highlighting

The extension provides syntax highlighting for Liquid template files (`.liquid`) in Visual Studio. The syntax highlighting uses HTML as the base document type, ensuring you get proper highlighting for both HTML and Liquid syntax within your templates.

### ⚡ Quick Commands

The extension adds two convenient commands to your project context menu (`.csproj`), located towards the top:

- **Add LiquidPage** - Quickly add a new Liquid page template to your project
- **Add LiquidErrorPage** - Add a new Liquid error page template to your project

## Prerequisites

**Important:** Before using the Add LiquidPage or Add LiquidErrorPage commands, you must install the `Kinetq.LiquidPages.Templates` dotnet CLI package.

To install the templates package, run the following command in your terminal:

```bash
dotnet new install Kinetq.LiquidPages.Templates
```

Without this package installed, the Add LiquidPage and Add LiquidErrorPage commands will not function correctly.

## How to Use

1. **Install the prerequisites**: Make sure you have installed the `Kinetq.LiquidPages.Templates` package as described above.

2. **Access the commands**: Right-click on your C# Project in Solution Explorer. You'll find the "Add LiquidPage" and "Add LiquidErrorPage" commands near the top of the context menu.

3. **Create templates**: Select the desired command and follow the prompts to add new Liquid template files to your project.

4. **Edit templates**: Open any `.liquid` file to take advantage of syntax highlighting.

## About Liquid Templates

Liquid is a flexible and safe templating language created by Shopify. This extension brings first-class support for Liquid templates to Visual Studio, making it easier to work with Liquid-based web applications in the .NET ecosystem.

## Feedback and Issues

If you encounter any issues or have suggestions for improvements, please visit the project repository to report them.
