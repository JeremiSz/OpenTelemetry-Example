import http from 'k6/http';

export const options = {
  duration: "60m",
  vus: 3
}

export default function () {
  const url = 'http://localhost:5270/api/student';

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  http.get(url, params);
}