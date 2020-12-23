import java.io.File
import java.io.IOException
import kotlin.system.*
import com.github.kittinunf.fuel.httpGet
import com.github.kittinunf.result.Result
import com.github.ajalt.clikt.core.*
import com.github.ajalt.clikt.parameters.options.*
import com.github.ajalt.clikt.parameters.types.*
import com.github.kittinunf.fuel.gson.responseObject
import com.google.gson.*

data class ComicResponse(
    val month: String,
    val num: Int,
    val link: String,
    val year: String,
    val news: String,
    val safeTitle: String,
    val transcript: String,
    val alt: String,
    val img: String,
    val title: String,
    val day: String
)

fun getComicResponse(number: Int): ComicResponse? {
    val url = when (number) {
        -1 -> "http://xkcd.com/info.0.json"
        else -> "http://xkcd.com/%d/info.0.json".format(number)
    }
    val gson = GsonBuilder().setFieldNamingPolicy(FieldNamingPolicy.LOWER_CASE_WITH_UNDERSCORES).create()
    val (_, _, result) = url.httpGet().responseObject<ComicResponse>(gson)
    return when (result) {
        is Result.Failure -> {
            println(result.getException().message)
            null
        }
        is Result.Success -> {
            result.get()
        }
    }
}

fun printComicResponse(comic: ComicResponse, outputFormat: String) {
    if (outputFormat == "json") {
        val gson = GsonBuilder().setPrettyPrinting().create()
        println(gson.toJson(comic))
    } else {
        println("Title: %s".format(comic.title))
        println("Alt: %s".format(comic.alt))
        println("Img: %s".format(comic.img))
    }
}

fun saveComic(comic: ComicResponse) {
    val (_, _, result) = comic.img.httpGet().response()
    when (result) {
        is Result.Failure -> {
            println(result.getException().message)
            return
        }
        is Result.Success -> {
            val filename = "xkcd_" + comic.img.split("/").last()
            try {
                File(filename).writeBytes(result.value)
            } catch (e: IOException) {
                println(e.message)
            }
        }
    }
}

fun validateOutput(outputFormat: String): Boolean {
    return listOf("text", "json").contains(outputFormat)
}

class GetXKCD : CliktCommand() {
    val number by option("-n", help="comic number to retrieve, latest by default").int().default(-1)
    val output by option("-o", help="output format: text or json").default("text")
    val save by option("-s", help="save comic locally").flag(default=false)

    override fun run() {
        if (!validateOutput(output)) {
            println("Unrecognized output format: %s".format(output))
            exitProcess(1)
        }
        val cr = getComicResponse(number)
        if (cr != null) {
            printComicResponse(cr, output)
            if (save) {
                saveComic(cr)
            }
        }
    }
}

fun main(args: Array<String>) = GetXKCD().main(args)