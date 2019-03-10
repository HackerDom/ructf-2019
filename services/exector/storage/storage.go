package storage

import (
	"errors"
	"fmt"
	"io/ioutil"
	"math"
	"path"
)

type Storage struct {
	DirName string
	BlockSize uint
	MaxItemsCount uint
}

func (storage *Storage) Init(dirName string, maxItemsCount uint) {
	storage.DirName = dirName
	CreateDirIfNotExists(dirName)
	storage.MaxItemsCount = maxItemsCount

	storage.BlockSize = uint(math.Sqrt(float64(maxItemsCount)))
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
