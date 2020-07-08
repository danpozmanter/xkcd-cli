name := "GetXKCD"

version := "0.0.1"

scalaVersion := "2.13.3"

ThisBuild / scalaVersion := "2.13.3"
ThisBuild / organization := "getxkcd"

resolvers += JavaNet2Repository

libraryDependencies += "com.lihaoyi" %% "requests" % "0.5.1"
libraryDependencies += "com.google.code.gson" % "gson" % "2.8.1"
libraryDependencies += "org.rogach" %% "scallop" % "3.5.0"

mainClass in (Compile, run) := Some("getxkcd.Main")
