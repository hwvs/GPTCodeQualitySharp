# GPTCodeQualitySharp (*-alpha_test_version*)

__GPTCodeQualitySharp__ is a language-agnostic code quality assessment tool that utilizes OpenAI GPT (**default=`GPT-3.5-Turbo`**, *TODO: GPT4*) or compatible text-completion models to analyze and score source code based on multiple conventions, via a large prompt.

This project aims to help developers pin-point areas with poor code quality and improve their projects.

Code is split into workable (truncated) chunks, and analyzed chunk-by-chunk - multiple pass-throughs using different chunk sizes (non-factors of each other) can allow the differential, or average, to be used to identify the precise areas, which avoids prompting for exact lines to flag.

__It is a work-in-progress__ and will be refactored for better performance and usability. All Classes are subject to future changes.

*(This is only on GitHub to get it out to the public faster, it's not final)*

## Credits
#### **Original Author**: [Hunter Watson](https://github.com/hwvs)
#### **Original Repo**: https://github.com/hwvs/GPTCodeQualitySharp
#### **Code License**: Mozilla Public License 2.0
## Contributions / Improvements
#### 🏆 If you make any positive changes, please [submit a pull request](https://github.com/hwvs/GPTCodeQualitySharp/pulls)!

---

## Tested Languages (Target-Language Analyzed)
- C#/.NET (CSharp) **`High Accuracy`**
- PHP **`High Accuracy`**
- Ruby **`High Accuracy`**
- PowerShell **`High Accuracy`**
- JavaScript **`Moderate Accuracy`**
- VB.NET **`Moderate Accuracy`**
- HTML/CSS **`Low Accuracy`**
  
*(Disclaimer: Accuracy is fairly subjective, and is compared to my own human opinion. Good code was tested from popular and well-engineered GitHub repositories. Bad code came from random searches for bad code, some of my own code, and by modifying some of the good code to add code-smells such as long chains of useless if-statements. )*

## Features

- Analyzes source code files and scores them based on multiple conventions
- Stores API calls in an SQLite database to prevent duplicate calls
- Uses multiple pass-throughs of the code to accurately pinpoint poor areas
- Includes a demo application to showcase the API

## (TODO) Future Improvements:

- Improve the scoring algorithm for better accuracy
- Refactor the codebase, using itself to see how it performs and to fine-tune the prompt
- Add more prompts
