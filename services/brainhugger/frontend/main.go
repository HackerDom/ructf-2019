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
const configPath = "config"
var config *Config

func RunTaskWrapper(writer http.ResponseWriter, request *http.Request) {
	if request.URL.Path == "/" {
		http.Redirect(writer, request, "/static/html", 301)
	}
	splitted := strings.Split(request.URL.Path, "/")
	if len(splitted) > 1 {
		if _, has := staticSubdirs[splitted[1]]; !has {
			if request.Method == http.MethodGet {
				url := fmt.Sprintf("http://%v:%v%v", config.ServerHost, config.BackendPort, request.URL.String())
				resp, err := http.Get(url)
				if err != nil {
					panic(err)
				}
				if _, err = io.Copy(writer, resp.Body); err != nil {
					panic(err)
				}
			} else if request.Method == http.MethodPost {
				url := fmt.Sprintf("http://%v:%v%v", config.ServerHost, config.BackendPort, request.URL.String())
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
	newConfig, err := ParseConfig(configPath)
	config = newConfig
	if err != nil {
		panic("can not parse config: " + err.Error())
	}
	staticSubdirs = make(map[string]bool)
	data, err := ioutil.ReadDir(config.StaticDir)
	if err != nil {
		panic(err)
	}
	for _, subdir := range data {
		if subdir.IsDir() {
			staticSubdirs[subdir.Name()] = true
		}
	}
	staticTempl := fmt.Sprintf("/%s/", config.StaticDir)
	http.Handle(staticTempl, http.StripPrefix(staticTempl, http.FileServer(http.Dir(staticTempl[1:]))))
	http.HandleFunc("/", RunTaskWrapper)
	log.Fatal(http.ListenAndServe(fmt.Sprintf("%s:%v", config.ServerHost, config.ServerPort), nil))
}
