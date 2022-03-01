import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  duration: "60m",
  vus: 1
}

export default function () {
  const url = 'http://localhost:5270/api/student';
  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };
  http.post(url, params);
  sleep(.33)
  //sleep(.167)
  //sleep(.11);
}