# GPTCodeQualitySharp

GPTCodeQualitySharp is a language-agnostic code quality assessment tool that utilizes OpenAI (**GPT-3.5-Rurbo**, *TODO: GPT4*) or compatible text-completion models to analyze and score source code based on multiple conventions. This project aims to help developers identify areas with poor code quality and improve their projects. It is a work-in-progress and will be refactored for better performance and usability.

## Credits
### Author: [Hunter Watson](https://github.com/hwvs)
### Original Repo: https://github.com/hwvs/GPTCodeQualitySharp
### License: Mozilla Public License 2.0
## Contributions / Improvements
### If you make any positive changes, please [submit a pull request](https://github.com/hwvs/GPTCodeQualitySharp/pulls)!

---

## Features

- Analyzes source code files and scores them based on multiple conventions
- Stores API calls in an SQLite database to prevent duplicate calls
- Uses multiple pass-throughs of the code to accurately pinpoint poor areas
- Includes a demo application to showcase the API

## (TODO) Future Improvements:

- Improve the scoring algorithm for better accuracy
- Refactor the codebase, using itself to see how it performs and to fine-tune the prompt
- Add more prompts
