package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net/http"
	"time"
)

type Task struct {
	Source string
	Stdin string
	Token string
}

type User struct {
	Password string
}

func addNewTask(source, stdin, token string) []byte {
	task := Task{
		Source: source,
		Stdin: stdin,
		Token: token,
	}
	data, err := json.Marshal(task)
	if err != nil {
		panic("can not marshal task: " + err.Error())
	}
	resp, err := http.Post("http://0.0.0.0:8080/run_task", "json", bytes.NewReader(data))
	if err != nil {
		panic("can not make post-request: " + err.Error())
	}
	if resp.StatusCode != 200 {
		panic(fmt.Sprintf("status code: %v", resp.StatusCode))
	}
	res, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		panic("can not read body: " + err.Error())
	}
	return res
}

func addNewUser(password string) []byte {
	user := User{
		Password: password,
	}
	data, err := json.Marshal(user)
	if err != nil {
		panic("can not marshal task: " + err.Error())
	}
	resp, err := http.Post("http://0.0.0.0:8080/register", "json", bytes.NewReader(data))
	if err != nil {
		panic("can not make post-request: " + err.Error())
	}
	if resp.StatusCode != 200 {
		panic(fmt.Sprintf("status code: %v", resp.StatusCode))
	}
	res, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		panic("can not read body: " + err.Error())
	}
	return res
}

func main() {
	start := time.Now()
	cnt := 1
	for i := 0; i < cnt; i++ {
		data := string(addNewTask("+", "", "TOKEN"))
		fmt.Println(data)
	}
	fmt.Println(float64(cnt) / (time.Now().Sub(start).Seconds()))
}
