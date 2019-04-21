#!/bin/bash

echo "run backend"
cd backend && ./backend&
echo "run frontend"
cd frontend && ./frontend

