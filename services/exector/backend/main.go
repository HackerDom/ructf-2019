package main

import (
	"encoding/json"
	"exector/backend/storage"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"runtime"
	"strconv"
)

var executor Executor
var tokensKeeper storage.Storage

type NewTask struct {
	Source string
	Stdin string
	Token string
}


func RunTask(w http.ResponseWriter, r *http.Request) {
	data, err := ioutil.ReadAll(r.Body)
	var newTask NewTask
	err = json.Unmarshal(data, &newTask)
	if err != nil {
		w.WriteHeader(400)
		return
	}
	taskId := executor.AddTask(newTask.Source, []byte(newTask.Stdin))
	if err := tokensKeeper.Set(taskId, []byte(newTask.Token)); err != nil {
		log.Printf("Can not save token while runnig a task, %v\n", err)
		w.WriteHeader(500)
		return
	}
	if _, err := w.Write([]byte(fmt.Sprintf("{\"taskId\": %v}", taskId))); err != nil {
		panic(err)
	}
}

func TaskInfo(w http.ResponseWriter, r *http.Request) {
	rawTaskId := r.URL.Path[len("/task_info/"):]
	taskId, err := strconv.ParseUint(rawTaskId, 10, 64)
	if err != nil {
		w.WriteHeader(400)
		return
	}
	if taskId >= uint64(executor.GetTaskCount()) {
		w.WriteHeader(404)
		return
	}
	token := r.URL.Query().Get("token")
	if token == "" {
		w.WriteHeader(403)
		return
	}
	expectedToken, err := tokensKeeper.Get(uint(taskId))
	if err != nil {
		log.Printf("Can not get token for task %v, due to %v\n", rawTaskId, err)
		w.WriteHeader(403)
		return
	}
	if string(expectedToken) != token {
		w.WriteHeader(403)
		return
	}
	rawTaskResult, err := executor.TaskInfo(uint(taskId))
	if err != nil {
		w.WriteHeader(404)
		return
	}
	if _, err := w.Write(rawTaskResult); err != nil {
		panic(err)
	}
}

func main() {
	runtime.GOMAXPROCS(4)
	executor.Init("tasks", "count", 200)
	tokensKeeper.Init("tokens", 40000)
	http.HandleFunc("/run_task", RunTask)
	http.HandleFunc("/task_info/", TaskInfo)
	log.Fatal(http.ListenAndServe(fmt.Sprintf("0.0.0.0:%d", 8080), nil))
}
