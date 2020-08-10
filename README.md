# Comparison (Python vs F# vs C# vs Scala vs Kotlin vs Clojure vs Nim)

Inspired by this post (using Go): https://eryb.space/2020/05/27/diving-into-go-by-building-a-cli-application.html

I decided to compare how it felt to build a super simple console app using a few different approaches:

* Python with a minimal approach
* Python with a more functional approach and static typing
* F#
* C#
* Scala
* Kotlin
* Clojure
* Nim

I took a few shortcuts, namely using Newtonsoft.Json for C#, not manually mapping safe_title to a more idiomatic field name like SafeTitle (in C#/F#). I also added in output format validation, rather than having a bad format default to text. I also skipped specifying a timeout manually, and organized everything in a single file to make comparison a bit easier. Otherwise I aimed to provide similar functionality.

I used the CommandLine package for both C# and F# to make comparison a bit easier, rather than choosing something a bit more idiomatic for F#.

TODO: Actually add in comments and spruce this all up.

This app is not for consumption or anything serious - more to get a feel for different languages.

PS Try getting comic 404!