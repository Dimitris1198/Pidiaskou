

import sys
import json
import numpy as np
try:
    from sklearn.metrics import silhouette_score
    from sklearn.cluster import KMeans
except ImportError as e:
    print("An ImportError occurred.")
    print(f"Name: {e.name}")
    sys.exit()

def optimal_k(x,kmax):
    sil = []
    for k in range(2, kmax+1):
        kmeans = KMeans(n_clusters = k).fit(x)
        labels = kmeans.labels_
        sil.append({
            "k":k,
            "score":silhouette_score(x, labels, metric = 'euclidean')
            })
    return max(sil,key=lambda x:x['score'])


def main():
    try:
        received = input()
        parsed_data= json.loads(received)
        cords=[]
        for cord in parsed_data['positions']:
            cords.append([cord['x'],cord['y'],cord['z']])
        to_np = np.array(cords)
        kmeans = KMeans(n_clusters=3,max_iter=500, random_state=0, n_init="auto").fit(to_np)
        sys.stdout.write(json.dumps({"peaks":kmeans.cluster_centers_.tolist()}))
    except Exception as err:
        sys.stdout.write(json.dumps({"error":str(err)}))
    
main()