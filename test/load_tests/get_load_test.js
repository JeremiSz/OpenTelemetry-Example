import http from 'k6/http';

export const options = {
		vus:3,
		duration:'60m',
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