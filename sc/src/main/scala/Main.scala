package todolist

import java.io.{BufferedOutputStream, FileOutputStream, IOException}
import com.google.gson.{GsonBuilder, FieldNamingPolicy}
import org.rogach.scallop._

case class ComicResponse(
  month: String,
  num: Int,
  link: String,
  year: String,
  news: String,
  safeTitle: String,
  transcript: String,
  alt: String,
  img: String,
  title: String,
  day: String
)

class ArgConf(arguments: Seq[String]) extends ScallopConf(arguments) {
  val number = opt[Int]()
  val save = opt[Boolean]()
  val outputFormat = opt[String]()
  verify()
}

object Main {

  def getComicResponse(number: Int): Option[ComicResponse] = {
    val url = number match {
      case -1 => "http://xkcd.com/info.0.json"
      case _ => "http://xkcd.com/%d/info.0.json".format(number)
    }
    val r = requests.get(url)
    if (!r.is2xx) {
      return None
    }
    val gson = new GsonBuilder().setFieldNamingPolicy(FieldNamingPolicy.LOWER_CASE_WITH_UNDERSCORES).create()
    Some(gson.fromJson(r.text(), classOf[ComicResponse]))
  }

  def printComicResponse(comic: ComicResponse, outputFormat: String): Unit = {
    if (outputFormat == "json") {
      val gson = new GsonBuilder().setPrettyPrinting().create()
      println(gson.toJson(comic))
    } else {
      println("Title: %s".format(comic.title))
      println("Alt: %s".format(comic.alt))
      println("Img: %s".format(comic.img))
    }
  }

  def saveComic(comic: ComicResponse) {
    val r = requests.get(comic.img)
    if (!r.is2xx) {
      println(r.text())
      return
    }
    val filename = "xkcd_" + comic.img.split("/").last
    try {
      val bos = new BufferedOutputStream(new FileOutputStream(filename))
      bos.write(r.data.array)
      bos.close()
    } catch {
      case e: IOException => println(e.getMessage())
    }
  }

  def validateOutput(outputFormat: String): Boolean = {
    List("text", "json").contains(outputFormat)
  }

  def main(args: Array[String]): Unit = {
    val argsC = new ArgConf(args)
    val cr = getComicResponse(argsC.number.getOrElse(-1))
    cr match {
      case Some(comic) => {
        val output = argsC.outputFormat.getOrElse("text")
        if (!validateOutput(output)) {
          println("Unrecognized output format: %s".format(output))
          System.exit(1)
        }
        printComicResponse(comic, output)
        if (argsC.save.getOrElse(false)) {
          saveComic(comic)
        }
      }
      case None => println("Error")
    }
  }
}
