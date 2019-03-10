package main

import (
	"bytes"
	"encoding/json"
	"errors"
	"exector/bfexecutor"
	"exector/storage"
	"fmt"
	"io/ioutil"
	"log"
	"os"
	"strconv"
)

type Executor struct {
	CounterFilename string
	TasksStorage storage.Storage
}

const (
	DONE = iota
	RUNNING = iota
	ERROR = iota
)

var Status2String = map[int]string {
	0: "DONE",
	1: "RUNNING",
	2: "ERROR",
}

type TaskResult struct {
	Status int
	Stdout string
	Error string
}

func (tr TaskResult) String() string {
	return fmt.Sprintf(
		"TaskResult(status=%v, stdout='%v', error='%v')",
		Status2String[tr.Status],
		tr.Stdout,
		tr.Error,
		)
}

func jsonMarshalWithoutEscaping(t interface{}) ([]byte, error) {
	buffer := &bytes.Buffer{}
	encoder := json.NewEncoder(buffer)
	encoder.SetEscapeHTML(false)
	err := encoder.Encode(t)
	return buffer.Bytes(), err
}

func (exec *Executor) SaveTask(result TaskResult, taskId uint) {
	rawTaskResult, err := jsonMarshalWithoutEscaping(result)
	if err != nil {
		log.Printf("Can not save task due to error: %v\n", err)
		return
	}
	err = exec.TasksStorage.Set(taskId, rawTaskResult)
	if err != nil {
		log.Printf("Can not save task due to error: %v\n", err)
		return
	}
}

func ExecuteTask(source, stdin string) (string, error) {
	result, err := bfexecutor.RunBfCode(source, stdin, 50000)
	return result, err
}

func (exec *Executor) ProcessTask(taskId uint, source, stdin string) {
	stdout, err := ExecuteTask(source, stdin)
	var result TaskResult
	if err != nil {
		result = TaskResult{ERROR, "", fmt.Sprint(err)}
	} else {
		result = TaskResult{DONE, stdout, ""}
	}
	exec.SaveTask(result, taskId)
}

func (exec *Executor) AddTask(source, stdin string) uint {
	//exec.createNewTaskDirIfNeed()
	taskId := exec.GetTaskCount()

	go exec.ProcessTask(taskId, source, stdin)
	exec.IncTaskCount()
	return taskId
}

func (exec *Executor) TaskInfo(taskId uint) ([]byte, error) {
	if taskId >= exec.GetTaskCount() {
		return nil, errors.New("no such task")
	}
	value, err := exec.TasksStorage.Get(taskId)
	if err != nil {
		return []byte("{\"Status\": \"1\"}"), nil
	} else {
		return value, nil
	}
}

func (exec *Executor) GetTaskCount() uint {
	if exec.createCounterFileIfNotExists() {
		return 0
	} else {
		data, err := ioutil.ReadFile(exec.CounterFilename)
		if err != nil {
			panic(err)
		}
		taskCount, err := strconv.ParseUint(string(data), 10, 64)
		if err != nil {
			panic(err)
		}
		return uint(taskCount)
	}
}

func (exec *Executor) IncTaskCount() {
	taskCount := exec.GetTaskCount() + 1
	err := ioutil.WriteFile(exec.CounterFilename, []byte(fmt.Sprint(taskCount)), 0777)
	if err != nil {
		panic(err)
	}
}

func (exec *Executor) createCounterFileIfNotExists() bool {
	if _, err := os.Stat(exec.CounterFilename); os.IsNotExist(err) {
		file, err := os.Create(exec.CounterFilename)
		defer file.Close()
		if err != nil {
			panic(err)
		}
		if _, err = file.Write([]byte("0")); err != nil {
			panic(err)
		}

		return true
	} else {
		return false
	}
}

//func (exec *Executor) createNewTaskDirIfNeed() {
//	dirFilename := path.Join(exec.TaskDir, fmt.Sprint(exec.GetTaskCount() / exec.TaskBlockSize))
//	storage.CreateDirIfNotExists(dirFilename)
//}

func (exec *Executor) Init(taskDir, counterFilename string, taskBlockSize uint) {
	exec.CounterFilename = counterFilename
	exec.TasksStorage.Init(taskDir, 40000)
	exec.createCounterFileIfNotExists()
}
