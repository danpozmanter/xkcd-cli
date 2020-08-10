(ns getxkcd.core
  (:gen-class)
  (:require [clojure.java.io])
  (:require [clojure.string :as str])
  (:require [clj-http.client :as client])
  (:require [clojure.tools.cli :refer [parse-opts]])
  (:require [clojure.data.json :as json]))

(def cli-options
  [["-n" "--number NUMBER" "Comic number to retrieve"
    :default -1
    :parse-fn #(Integer/parseInt %)
    :validate [#(< 0 % 0x10000) "Must be a number between 1 and 65536"]]
   ["-o" "--output FORMAT" "Output format (json or text)"
    :default "text"
    :validate [#(or (= % "json") (= % "text")) "Output format not recognized"]]
   ["-s" nil "Save"
    :id :save
    :default false]
   ["-h" "--help"]])

(defn get-comic-response [number]
  (let [url
    (if (= number -1)
      "http://xkcd.com/info.0.json"
      (str "http://xkcd.com/" number "/info.0.json"))]
  (try
    (json/read-str (get (client/get url) :body))
    (catch Exception e (println (str "Web Error: " (.getMessage e)))))))

(defn print-comic-response [comic output-format]
  (if (= output-format "json")
    (println (json/write-str comic))
    (do
      (println (get comic "title"))
      (println (get comic "alt"))
      (println (get comic "img")))))

(defn save-comic [comic]
  (println "Saving comic...")
  (let [image-url (get comic "img")
        filename (str "xkcd_" (last (str/split image-url #"/")))
        image (try
                (get (client/get image-url {:as :byte-array}) :body)
                (catch Exception e (
                                     (println (.getMessage e)))
                       (System/exit 1)))]
    (with-open [w (clojure.java.io/output-stream filename)]
      (.write w image))))

(defn handle-comic [args]
  (let [comic (get-comic-response (get-in args [:options :number]))]
  (print-comic-response comic (get-in args [:options :output]))
  (if (get-in args [:options :save]) (save-comic comic))
  ))

(defn -main
  ""
  [& raw-args]
  (let [args (parse-opts raw-args cli-options)]
  (when (get args :errors)
    (println (get args :errors))
    (System/exit 1))
  (handle-comic args)))
