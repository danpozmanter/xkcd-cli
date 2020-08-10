(defproject cl "0.0.1"
  :description "Get XKCD Clojure example"
  :dependencies [
    [org.clojure/clojure "1.8.0"],
    [clj-http "3.10.1"],
    [org.clojure/tools.cli "1.0.194"],
    [org.clojure/data.json "1.0.0"]
  ]
  :main ^:skip-aot getxkcd.core
  :target-path "target/%s"
  :profiles {:uberjar {:aot :all}})
