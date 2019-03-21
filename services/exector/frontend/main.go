package main

import (
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"net/http"
	"strings"
)

var staticSubdirs map[string]bool

func RunTaskWrapper(writer http.ResponseWriter, request *http.Request) {
	splitted := strings.Split(request.URL.Path, "/")
	if len(splitted) > 1 {
		if _, has := staticSubdirs[splitted[1]]; !has {
			if request.Method == http.MethodGet {
				url := fmt.Sprintf("http://%v:%v%v", "0.0.0.0", 8080, request.URL.String())
				resp, err := http.Get(url)
				if err != nil {
					panic(err)
				}
				if _, err = io.Copy(writer, resp.Body); err != nil {
					panic(err)
				}
			} else if request.Method == http.MethodPost {
				url := fmt.Sprintf("http://%v:%v%v", "0.0.0.0", 8080, request.URL.String())
				resp, err := http.Post(url, "text", request.Body)
				if err != nil {
					panic(err)
				}
				if _, err = io.Copy(writer, resp.Body); err != nil {
					panic(err)
				}
			}
		}
	}
}

func main() {
	staticSubdirs = make(map[string]bool)
	data, err := ioutil.ReadDir("static")
	if err != nil {
		panic(err)
	}
	for _, subdir := range data {
		if subdir.IsDir() {
			staticSubdirs[subdir.Name()] = true
		}
	}

	http.Handle("/static/", http.StripPrefix("/static/", http.FileServer(http.Dir("static/"))))
	http.HandleFunc("/", RunTaskWrapper)
	log.Fatal(http.ListenAndServe(":3000", nil))
}
