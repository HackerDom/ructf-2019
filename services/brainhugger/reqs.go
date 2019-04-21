package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net/http"
	"sync"
	"time"
)

type Task struct {
	Source string
	Stdin  string
	Token  string
	OwnerId uint
}

type User struct {
	Password string
}

func addNewTask(wg *sync.WaitGroup, source, stdin, token string, owner uint) []byte {
	defer wg.Done()
	task := Task{
		Source:  source,
		Stdin:   stdin,
		Token:   token,
		OwnerId: owner,
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

func addNewUser(wg *sync.WaitGroup, password string) []byte {
	defer wg.Done()
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
	var wg sync.WaitGroup
	start := time.Now()
	cnt := 500
	for i := 0; i < cnt; i++ {
		wg.Add(1)
		go addNewTask(&wg, "+++++++", "", "token", 0)
	}
	wg.Wait()
	fmt.Println(float64(cnt) / (time.Now().Sub(start).Seconds()))
}
