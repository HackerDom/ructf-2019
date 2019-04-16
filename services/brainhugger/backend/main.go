package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"strconv"
)

var taskManager TasksManager
var usersManager UsersManager
const configPath = "config"

type NewTask struct {
	Source string
	Stdin string
	Token string
}

type NewUser struct {
	Password string
}

type TaskResponse struct {
	Stdout string
	Status int
	Error string
}

func handleRunTask(w http.ResponseWriter, r *http.Request) {
	data, err := ioutil.ReadAll(r.Body)
	if err != nil {
		w.WriteHeader(400)
		return
	}
	var newTask NewTask
	err = json.Unmarshal(data, &newTask)
	if err != nil {
		w.WriteHeader(400)
		return
	}
	taskId := taskManager.AddTask(newTask.Source, newTask.Token, []byte(newTask.Stdin), 0)
	if _, err := w.Write([]byte(fmt.Sprintf("{\"taskId\": %v}", taskId))); err != nil {
		panic(err)
	}
}

func handleTaskInfo(w http.ResponseWriter, r *http.Request) {
	rawTaskId := r.URL.Path[len("/task_info/"):]
	taskId, err := strconv.ParseUint(rawTaskId, 10, 64)
	if err != nil {
		w.WriteHeader(400)
		return
	}
	if taskId >= uint64(taskManager.TaskStorage.GetItemsCount()) {
		w.WriteHeader(404)
		return
	}
	task, err := taskManager.TaskInfo(uint(taskId))
	if err != nil {
		w.WriteHeader(404)
		return
	}
	token := r.URL.Query().Get("token")
	if token == "" {
		w.WriteHeader(403)
		return
	}
	if task.Token != token {
		w.WriteHeader(403)
		return
	}
	taskResponse := TaskResponse{
		Stdout: task.Result.Stdout,
		Error: task.Result.Error,
		Status: task.Status,
	}
	rawTaskResponse, err := JsonMarshalWithoutEscaping(taskResponse)
	if err != nil {
		log.Printf("Can not marshal task response: %v\n", taskResponse)
		w.WriteHeader(500)
		return
	}
	if _, err := w.Write(rawTaskResponse); err != nil {
		w.WriteHeader(400)
		log.Println("Can not write task response: " + err.Error())
	}
}


func handleRegUser(w http.ResponseWriter, r *http.Request) {
	data, err := ioutil.ReadAll(r.Body)
	if err != nil {
		w.WriteHeader(400)
		return
	}
	var newUser NewUser
	err = json.Unmarshal(data, &newUser)
	if err != nil {
		w.WriteHeader(400)
		return
	}
	userId, err := usersManager.AddUser(newUser.Password)
	if err != nil {
		w.WriteHeader(400)
		return
	}
	if _, err := w.Write([]byte(fmt.Sprintf("{\"userId\": %v}", userId))); err != nil {
		panic(err)
	}
}

func main() {
	config, err := ParseConfig(configPath)
	if err != nil {
		panic("can not parse config: " + err.Error())
	}
	if err := taskManager.Init(config.TasksDir, config.BrainHugExecutorPath); err != nil {
		panic(err)
	}
	usersManager.Init(config.UsersDir, config.MaxItemsCount)
	http.HandleFunc("/run_task", handleRunTask)
	http.HandleFunc("/task_info/", handleTaskInfo)
	http.HandleFunc("/register", handleRegUser)
	log.Fatal(http.ListenAndServe(fmt.Sprintf("%s:%d", config.ServerHost, config.ServerPort), nil))
}
