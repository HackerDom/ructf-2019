package main

import (
	"bytes"
	"encoding/json"
	"errors"
	"brainhugger/backend/bhexecutor"
	"brainhugger/backend/storage"
	"log"
)

const MaxBhOperations = 50000

const (
	DONE = 0
	RUNNING = 1
	ERROR = 2
)

var Status2String = map[int]string {
	0: "DONE",
	1: "RUNNING",
	2: "ERROR",
}

type TaskResult struct {
	Stdout []byte
	Error string
}

type Task struct {
	Token   string
	OwnerId uint
	Result  TaskResult
	Status  int
}

type TasksManager struct {
	TaskStorage storage.Storage
	BhExecutor  bhexecutor.BhExecutor
}

func (tm *TasksManager) Init(taskDir string, bhExecutorBinPath string, maxItemsCount uint) error {
	tm.TaskStorage.Init(taskDir, maxItemsCount)
	if err := tm.BhExecutor.Init(bhExecutorBinPath); err != nil {
		return errors.New("can not init bhexecutor: " + err.Error())
	}
	return nil
}

func JsonMarshalWithoutEscaping(t interface{}) ([]byte, error) {
	buffer := &bytes.Buffer{}
	encoder := json.NewEncoder(buffer)
	encoder.SetEscapeHTML(false)
	err := encoder.Encode(t)
	return buffer.Bytes(), err
}

func (tm *TasksManager) SaveTask(taskId uint, task *Task) {
	rawTaskResult, err := JsonMarshalWithoutEscaping(*task)
	if err != nil {
		log.Printf("Can not save task due to error: %v\n", err)
		return
	}
	if err = tm.TaskStorage.Set(taskId, rawTaskResult); err != nil {
		log.Printf("Can not save task due to error: %v\n", err)
		return
	}
}

func (tm *TasksManager) ExecuteTask(source string, stdin []byte) ([]byte, error) {
	result, err := tm.BhExecutor.RunBhCode(source, stdin, MaxBhOperations)
	return result, err
}

func (tm *TasksManager) AddTask(source, token string, stdin []byte, ownerId uint) uint {
	taskId := tm.TaskStorage.GetItemsCount()
	go tm.ProcessTask(taskId, source, token, stdin, ownerId)
	tm.TaskStorage.IncItemsCount()
	return taskId
}

func (tm *TasksManager) ProcessTask(taskId uint, source, token string, stdin []byte, ownerId uint) {
	task := Task{
		Status:  1,
		Token:   token,
		OwnerId: ownerId,
	}
	tm.SaveTask(taskId, &task)
	stdout, err := tm.ExecuteTask(source, stdin)
	if err != nil {
		task.Status = ERROR
		task.Result.Error = err.Error()
	} else {
		task.Status = DONE
		task.Result.Stdout = stdout
	}
	tm.SaveTask(taskId, &task)
}

func (tm *TasksManager) TaskInfo(taskId uint) (*Task, error) {
	if taskId >= tm.TaskStorage.GetItemsCount() {
		return nil, errors.New("no such task")
	}
	rawTask, err := tm.TaskStorage.Get(taskId)
	if err != nil {
		return nil, errors.New("no such task")
	}
	var task Task
	err = json.Unmarshal(rawTask, &task)
	if err != nil {
		return nil, errors.New("can not read task")
	}
	return &task, nil
}
