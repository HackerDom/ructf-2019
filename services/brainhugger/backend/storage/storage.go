package storage

import (
	"errors"
	"fmt"
	"io/ioutil"
	"math"
	"os"
	"path"
	"strconv"
	"sync"
)

const CounterFilename = "count"

type Storage struct {
	DirName string
	CounterFilename string
	BlockSize uint
	MaxItemsCount uint
	Locker sync.Mutex
}

func (storage *Storage) Init(dirName string, maxItemsCount uint) {
	storage.DirName = dirName
	CreateDirIfNotExists(dirName)
	storage.MaxItemsCount = maxItemsCount
	storage.BlockSize = uint(math.Sqrt(float64(maxItemsCount)))
	storage.CounterFilename = path.Join(dirName, CounterFilename)
}

func (storage *Storage) GetItemFilename(itemId uint) string {
	subdirName := fmt.Sprint(itemId / storage.BlockSize)
	filename := fmt.Sprint(itemId % storage.BlockSize)
	return path.Join(storage.DirName, subdirName, filename)
}

func (storage *Storage) Set(itemId uint, value []byte) error {
	CreateDirIfNotExists(path.Join(storage.DirName, fmt.Sprint(itemId / storage.BlockSize)))
	err := ioutil.WriteFile(storage.GetItemFilename(itemId), value, 0644)
	if err != nil {
		return errors.New(fmt.Sprintf("can not set value: %v", err))
	}
	return nil
}

func (storage *Storage) Get(itemId uint) ([]byte, error) {
	data, err := ioutil.ReadFile(storage.GetItemFilename(itemId))
	if err != nil {
		return nil, err
	}
	return data, nil
}

func (storage *Storage) createCounterFileIfNotExists() bool {
	if _, err := os.Stat(storage.CounterFilename); os.IsNotExist(err) {
		file, err := os.Create(storage.CounterFilename)
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

func (storage *Storage) getItemsCount(noBlock bool) uint {
	if !noBlock {
		storage.Locker.Lock()
		defer storage.Locker.Unlock()
	}
	if storage.createCounterFileIfNotExists() {
		return 0
	} else {
		data, err := ioutil.ReadFile(storage.CounterFilename)
		if err != nil {
			panic(err)
		}
		itemsCount, err := strconv.ParseUint(string(data), 10, 64)
		if err != nil {
			panic(err)
		}
		return uint(itemsCount)
	}
}

func (storage *Storage) GetItemsCount() uint {
	return storage.getItemsCount(false)
}

func (storage *Storage) IncItemsCount() {
	storage.Locker.Lock()
	defer storage.Locker.Unlock()
	itemsCount := storage.getItemsCount(true) + 1
	err := ioutil.WriteFile(storage.CounterFilename, []byte(fmt.Sprint(itemsCount)), 0777)
	if err != nil {
		panic(err)
	}
}
