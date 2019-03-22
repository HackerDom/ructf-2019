package main

/*
#include<unistd.h>
void suicide() {
	*(int*)0 = 0;
}
*/
import "C"
import (
	"bytes"
	"encoding/json"
	"errors"
	"exector/backend/bfwrapper"
	"exector/backend/storage"
	"fmt"
	"io/ioutil"
	"log"
	"os"
	"strconv"
)

type TaskExecutor struct {
	CounterFilename string
	TasksStorage storage.Storage
	BfExecutor bfwrapper.BfExecutor
}

const (
	DONE = 0
	ERROR = 2
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

func (exec *TaskExecutor) SaveTask(result TaskResult, taskId uint) {
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

func (exec *TaskExecutor) ExecuteTask(source string, stdin []byte) ([]byte, error) {
	result, err := exec.BfExecutor.RunBfCode(source, stdin, 50000)
	return result, err
}

func (exec *TaskExecutor) ProcessTask(taskId uint, source string, stdin []byte) {
	exec.SaveTask(TaskResult{2, "", "Unexpected error"}, taskId)
	stdout, err := exec.ExecuteTask(source, stdin)
	var result TaskResult
	if err != nil {
		result = TaskResult{ERROR, "", fmt.Sprint(err)}
	} else {
		result = TaskResult{DONE, string(stdout), ""}
	}
	exec.SaveTask(result, taskId)
}

func (exec *TaskExecutor) AddTask(source string, stdin []byte) uint {
	taskId := exec.GetTaskCount()
	go exec.ProcessTask(taskId, source, stdin)
	exec.IncTaskCount()
	return taskId
}

func (exec *TaskExecutor) TaskInfo(taskId uint) ([]byte, error) {
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

func (exec *TaskExecutor) GetTaskCount() uint {
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

func (exec *TaskExecutor) IncTaskCount() {
	taskCount := exec.GetTaskCount() + 1
	err := ioutil.WriteFile(exec.CounterFilename, []byte(fmt.Sprint(taskCount)), 0777)
	if err != nil {
		panic(err)
	}
}

func (exec *TaskExecutor) createCounterFileIfNotExists() bool {
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

func (exec *TaskExecutor) Init(taskDir, counterFilename string, taskBlockSize uint, bfExecutorBinPath string) error {
	exec.CounterFilename = counterFilename
	exec.TasksStorage.Init(taskDir, 40000)
	exec.createCounterFileIfNotExists()
	var bfExecutor bfwrapper.BfExecutor
	if err := bfExecutor.Init(bfExecutorBinPath); err != nil {
		return err
	}
	exec.BfExecutor = bfExecutor
	return nil
}
