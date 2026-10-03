# Third-party notices

The component pages and examples under `Pages/Components/`, the sample data under `ExamplesData/`
and the photos under `wwwroot/images/` are taken from the MudBlazor documentation site
(<https://github.com/MudBlazor/MudBlazor>, tag `v9.10.0`: `src/MudBlazor.Docs/Pages/Components`,
`src/MudBlazor.Examples.Data`, `src/MudBlazor.Docs.Wasm/wwwroot/images`). They are copied by
`sync-mudblazor-docs.sh`, which also lists every edit the port makes: page links rewritten to be
relative or to point at mudblazor.com, one example's internal localizer swapped for the public
interceptor, and two namespace/injection adjustments. `Services/ExampleSource.cs`,
`Services/MenuService.cs`, `Docs/T.cs` and the components under `Docs/` are adapted from
MudBlazor.Docs. All of it is used under MudBlazor's licence:

```
MIT License

Copyright (c) 2021 MudBlazor

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
