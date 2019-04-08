package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"time"
)

type Task struct {
	Source string
	Stdin string
	Token string
}


func main() {
	task := Task{
		Source: "+[>+.]",
		Stdin: ".",
		Token: "TOKEN",
	}
	data, err := json.Marshal(task)
	if err != nil {
		panic(err)
	}
	start := time.Now()
	cnt := 1000
	for i := 0; i < cnt; i++ {
		resp, err := http.Post("http://0.0.0.0:8080/run_task", "json", bytes.NewReader(data))
		if err != nil {
			panic(err)
		}
		if resp.StatusCode != 200 {
			fmt.Println(resp)
			panic("unsuccessfult request")
		}
	}
	fmt.Println("rps", float64(cnt) / time.Now().Sub(start).Seconds())
}
