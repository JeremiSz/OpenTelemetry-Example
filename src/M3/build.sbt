name := """M3"""
organization := "CCS.OpenTelemetry"

version := "1.0-SNAPSHOT"

lazy val root = (project in file(".")).enablePlugins(PlayJava)

scalaVersion := "2.13.6"

libraryDependencies += guice
libraryDependencies += "io.opentelemetry" % "opentelemetry-sdk-trace" % "1.10.1"
libraryDependencies += "io.opentelemetry" % "opentelemetry-sdk" % "1.10.1"
libraryDependencies += "io.opentelemetry" % "opentelemetry-api" % "1.10.1"
libraryDependencies += "io.opentelemetry.instrumentation" % "opentelemetry-otlp-exporter-starter" % "1.10.1-alpha"