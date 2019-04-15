ulimit -n 10000
echo "run backend"
cd backend && ./backend&
echo "run frontend"
cd frontend && ./frontend

